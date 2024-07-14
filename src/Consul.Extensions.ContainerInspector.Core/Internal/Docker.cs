using Consul.Extensions.ContainerInspector.Core.Configuration.Models;
using Consul.Extensions.ContainerInspector.Core.Models;
using Consul.Extensions.ContainerInspector.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Consul.Extensions.ContainerInspector.Core.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IDocker" />.
    /// </summary>
    internal class Docker(
        ILogger<IDocker>? dockerLogger,
        IOptions<DockerConfiguration> options,
        IHttpClientFactory clientFactory) : IDocker
    {
        private static readonly string[] _supportedEventTypes = ["container", "network"];
        private static readonly string _baseRequestUri = $"/v{IDocker.DockerVersion}";

        public async Task<IEnumerable<DockerContainer>> GetContainersAsync(CancellationToken cancellationToken)
        {
            var containerFilters = new Dictionary<string, string[]?> { { "label", options.Value.ExpectedLabels } };
            var containers = await GetAsync<IEnumerable<DockerContainerResponse>>(
                "containers/json", containerFilters, cancellationToken);

            // We will only return containers that are at least running. There is no need to return
            // stopped containers, since most likely the Consul does not have information about them,
            // or if there is any, it will be deleted after the inspector determines this.
            return (containers ?? []).Where(container => container.State == "running").Select(Convert);
        }

        public async Task<DockerContainer?> GetContainerAsync(string containerId, CancellationToken cancellationToken)
        {
            var resourceUri = $"containers/{containerId}/json";

            // Compared to the GetContainersAsync method, this method returns information about
            // the container regardless of whether it is running.
            var container = await GetAsync<DockerContainerDataResponse>(resourceUri, default, cancellationToken);
            if (container == default)
            {
                dockerLogger?.DockerContainerNotFound(containerId);
                return default;
            }

            var containerLabels = container.Configuration.Labels;
            foreach (var data in (options.Value.ExpectedLabels ?? []).Select(value => value.Split('=', count: 2)))
            {
                if (!containerLabels.TryGetValue(data[0], out string? value))
                {
                    dockerLogger?.DockerContainerExpectedLabelMissing(containerId, data[0]);
                    return default;
                }

                // The expected label can be specified in the format name=value. In such cases, in
                // addition to checking whether the label is present in the container, we need to
                // check whether the value of the container's label matches the expected value.
                if (data.Length == 2 && data[1] != value)
                {
                    dockerLogger?.DockerContainerExpectedLabelMissing(containerId, data[0], data[1]);
                    return default;
                }
            }

            return Convert(container);
        }

        public async IAsyncEnumerable<DockerContainerEvent> MonitorAsync(
            DateTime since, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var resourceUri = $"events?since={((DateTimeOffset)since).ToUnixTimeSeconds()}";
            var resourceFilters = new Dictionary<string, string[]?>()
            {
                { "type", _supportedEventTypes },
                { "label", options.Value.ExpectedLabels }
            };

            using var requestClient = clientFactory.CreateClient(nameof(IDocker));
            using var requestMessage = CreateRequestMessage(resourceUri, resourceFilters);
            using var responseMessage = await requestClient.SendAsync(
                requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            responseMessage.EnsureSuccessStatusCode();
            using var contentStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);
            using var contentStreamReader = new StreamReader(contentStream, new UTF8Encoding(false));

            // The Docker API server will hold connections and when events occur, send a JSON string
            // for each event. There is always a line break at the end of a JSON string.
            while (!cancellationToken.IsCancellationRequested)
            {
                var responseContent = await contentStreamReader.ReadLineAsync(cancellationToken);
                if (responseContent == default || cancellationToken.IsCancellationRequested)
                {
                    continue;
                }

                var response = JsonSerializer.Deserialize<DockerEventResponse>(responseContent);
                if (response != default && _supportedEventTypes.Contains(response.Type))
                {
                    dockerLogger?.DetectedDockerEvent(response.Type, response.Action, responseContent);

                    yield return new DockerContainerEvent
                    {
                        EventAction = response.Action,
                        EventType = response.Type,
                        ContainerId = response.Actor.Id,
                    };
                }
            }
        }

        /// <summary>
        /// Connects to Docker using Unix socket and returns a <see cref="NetworkStream" />.
        /// </summary>
        public static async ValueTask<Stream> ConnectAsync(EndPoint socketEndpoint, CancellationToken cancellationToken)
        {
            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            await socket.ConnectAsync(socketEndpoint, cancellationToken).ConfigureAwait(false);
            return new NetworkStream(socket, ownsSocket: false);
        }

        private async Task<T?> GetAsync<T>(
            string resourceUri, IDictionary<string, string[]?>? resourceFilters, CancellationToken cancellationToken)
        {
            using var requestClient = clientFactory.CreateClient(nameof(IDocker));
            using var requestMessage = CreateRequestMessage(resourceUri, resourceFilters);
            using var responseMessage = await requestClient.SendAsync(
                requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (responseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            responseMessage.EnsureSuccessStatusCode();
            using var contentStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(contentStream, cancellationToken: cancellationToken);
        }

        private static HttpRequestMessage CreateRequestMessage(string resourceUri, IDictionary<string, string[]?>? resourceFilters)
        {
            if (resourceFilters?.Count > 0)
            {
                // Before creating the request with resource filters, we will discard any filters
                // that have no values ​​and serialize the remaining resource filters to JSON format.
                // If for example the resource filters contain two labels, Docker will only return
                // containers that have both labels, not just one of them.
                resourceFilters = resourceFilters.Where(data => data.Value?.Length > 0).ToDictionary();
                if (resourceFilters?.Count > 0)
                {
                    var serializedFilters = JsonSerializer.Serialize(resourceFilters);
                    resourceUri += $"?filters={HttpUtility.UrlEncode(serializedFilters)}";
                }
            }

            return new HttpRequestMessage(HttpMethod.Get, string.Join('/', [_baseRequestUri, resourceUri]));
        }

        private static DockerContainer Convert(DockerContainerResponseBase response)
        {
            var containerNetworks = response.NetworkSettings.Networks.Select(network =>
            {
                return new KeyValuePair<string, IPAddress?>(
                    network.Key,

                    // The Docker API always sends a valid IP address or a null/empty string. So we
                    // only check if there is a value in the property and parse the IP address.
                    network.Value.IPAddress?.Length > 0 ? IPAddress.Parse(network.Value.IPAddress!) : default);
            });

            var containerState = response switch
            {
                DockerContainerResponse dockerResponse => dockerResponse.State,
                DockerContainerDataResponse dataResponse => dataResponse.State.Status,
                _ => default
            };

            var containerLabels = response switch
            {
                DockerContainerResponse dockerResponse => dockerResponse.Labels,
                DockerContainerDataResponse dataResponse => dataResponse.Configuration.Labels,
                _ => default
            };

            return new DockerContainer
            {
                Id = response.Id,
                State = containerState ?? string.Empty,
                Labels = containerLabels ?? new Dictionary<string, string>(),
                Networks = containerNetworks.ToDictionary()
            };
        }

        private abstract record DockerContainerResponseBase(
            [property: JsonRequired] string Id,
            [property: JsonRequired] DockerContainerNetworkSettingsResponse NetworkSettings);

        private record DockerContainerResponse(
            [property: JsonRequired] string State,
            [property: JsonRequired] IDictionary<string, string> Labels,
            string Id,
            DockerContainerNetworkSettingsResponse NetworkSettings) : DockerContainerResponseBase(Id, NetworkSettings);

        private record DockerContainerDataResponse(
            [property: JsonRequired] DockerContainerStateResponse State,
            [property: JsonRequired, JsonPropertyName("Config")] DockerContainerConfigurationResponse Configuration,
            string Id,
            DockerContainerNetworkSettingsResponse NetworkSettings) : DockerContainerResponseBase(Id, NetworkSettings);

        private record DockerContainerNetworkSettingsResponse(
            [property: JsonRequired] IDictionary<string, DockerContainerNetworkResponse> Networks);

        private record DockerContainerNetworkResponse(
            [property: JsonRequired] string? IPAddress);

        private record DockerContainerStateResponse(
            [property: JsonRequired] string Status);

        private record DockerContainerConfigurationResponse(
            [property: JsonRequired] IDictionary<string, string> Labels);

        private record DockerEventResponse(
            [property: JsonRequired] string Type,
            [property: JsonRequired] string Action,
            [property: JsonRequired] DockerEventActorResponse Actor);

        private record DockerEventActorResponse(
            [property: JsonRequired, JsonPropertyName("ID")] string Id,
            [property: JsonRequired] IDictionary<string, string> Attributes);
    }
}
