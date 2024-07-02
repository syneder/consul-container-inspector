using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Consul.Extensions.ContainerInspector.Internal
{
    public class Docker(IHttpClientFactory clientFactory)
    {
        public static readonly Version DockerVersion = new("1.43");

        /// <summary>
        /// Returns running containers and information needed to register services in Consul.
        /// </summary>
        /// <returns><see cref="Task{IEnumerable{DockerContainer}}" /> that completes with enumerate
        /// of the container data that contain information for registering services in Consul.</returns>
        public async Task<IEnumerable<DockerContainer>> GetContainersAsync(CancellationToken cancellationToken)
        {
            var dockerContainers = await GetAsync<IEnumerable<DockerContainerResponse>>(
                $"/v{DockerVersion}/containers/json", cancellationToken);

            // We will only return containers that are at least running. There is no need to return
            // stopped containers, since most likely the Consul does not have information about them,
            // or if there is any, it will be deleted after the inspector determines this.
            return (dockerContainers ?? []).Where(container => container.State == "running").Select(Convert);
        }

        /// <summary>
        /// Returns information about the container with the specified <paramref name="containerId"/>,
        /// needed to register service in Consul.
        /// </summary>
        /// <returns><see cref="Task{DockerContainer?}" /> that completes with container data that
        /// contain information for registering service in Consul.</returns>
        public async Task<DockerContainer?> GetContainerAsync(string containerId, CancellationToken cancellationToken)
        {
            // Compared to the GetContainersAsync method, this method returns information about
            // the container regardless of whether it is running.
            var dockerContainer = await GetAsync<DockerContainerDataResponse>(
                $"/v{DockerVersion}/containers/{containerId}/json", cancellationToken);

            return dockerContainer == default ? default : Convert(dockerContainer);
        }

        /// <summary>
        /// Monitors state change events for containers and the networks used by those containers.
        /// </summary>
        /// <returns>An infinite <see cref="IAsyncEnumerable{DockerContainerEvent}"/> of events that
        /// can only be interrupted by <paramref name="cancellationToken"/>.</returns>
        public async IAsyncEnumerable<DockerContainerEvent> MonitorAsync(
            DateTime since, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var client = clientFactory.CreateClient(nameof(Docker));
            using var responseMessage = await GetResponseMessageAsync(
                client, $"/v{DockerVersion}/events?since={((DateTimeOffset)since).ToUnixTimeSeconds()}", cancellationToken);

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
                if (response?.Type == "container" || response?.Type == "network")
                {
                    yield return new DockerContainerEvent
                    {
                        EventAction = response.Action,
                        EventType = response.Type,
                        ContainerId = response.Actor.Id,
                    };
                }
            }
        }

        private async Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken)
        {
            using var client = clientFactory.CreateClient(nameof(Docker));
            using var responseMessage = await GetResponseMessageAsync(client, requestUri, cancellationToken);
            if (responseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            responseMessage.EnsureSuccessStatusCode();
            using var contentStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(contentStream, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Creates and sends an HTTP GET request with the specified <paramref name="requestUri"/>
        /// and returns a response message once the server has sent the headers.
        /// </summary>
        private static async Task<HttpResponseMessage> GetResponseMessageAsync(
            HttpClient requestClient, string requestUri, CancellationToken cancellationToken)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
            return await requestClient.SendAsync(
                requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
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
            [property: JsonRequired] IDictionary<string, string>? Labels,
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
            [property: JsonRequired] IDictionary<string, string>? Labels);

        private record DockerEventResponse(
            [property: JsonRequired] string Type,
            [property: JsonRequired] string Action,
            [property: JsonRequired] DockerEventActorResponse Actor);

        private record DockerEventActorResponse(
            [property: JsonRequired, JsonPropertyName("ID")] string Id,
            [property: JsonRequired] IDictionary<string, string> Attributes);
    }
}
