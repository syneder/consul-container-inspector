using Consul.Extensions.ContainerInspector.Configurations.Models;
using Consul.Extensions.ContainerInspector.Core.Internal.Models;
using Consul.Extensions.ContainerInspector.Core.Models;
using Consul.Extensions.ContainerInspector.Extensions;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Consul.Extensions.ContainerInspector.Core.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IDockerClient" />.
    /// </summary>
    internal class DockerClient(
        IHttpClientFactory clientFactory,
        DockerConfiguration configuration,
        ILogger<IDockerClient>? clientLogger) : BaseClient(nameof(IDockerClient), clientFactory), IDockerClient
    {
        private static readonly string[] _supportedEventTypes = ["container", "network"];

        protected override string BaseResourceURI { get; } = $"v{IDockerClient.DockerVersion}";

        public async Task<IEnumerable<DockerContainer>> GetContainersAsync(CancellationToken cancellationToken)
        {
            var request = CreateRequest(HttpMethod.Get, "containers/json");
            AddContainerFilters(request, new() { { "label", configuration.ExpectedLabels } });

            var response = await request.ExecuteRequestAsync(
                JsonSerializerGeneratedContext.Default.DockerResponseCollection, cancellationToken);

            if (response?.Count > 0)
            {
                // We will only return containers that are at least running. There is no need to return
                // stopped containers, since most likely the Consul does not have information about them,
                // or if there is any, it will be deleted after the inspector determines this.
                return response.Where(container => container.State == "running").Select(Convert);
            }

            return [];
        }

        public async Task<DockerContainer?> GetContainerAsync(string containerId, CancellationToken cancellationToken)
        {
            var request = CreateRequest(HttpMethod.Get, $"containers/{containerId}/json");

            using (clientLogger?.CreateContainerScope(containerId))
            {
                try
                {
                    // Compared to the GetContainersAsync method, this method returns information about
                    // the container regardless of whether it is running.
                    var container = await request.ExecuteRequestAsync(
                        JsonSerializerGeneratedContext.Default.InspectedDockerResponse, cancellationToken);

                    if (container == default)
                    {
                        return default;
                    }

                    var containerLabels = container.Configuration.Labels;
                    if (containerLabels.Count > 0)
                    {
                        clientLogger?.DockerContainerContainsLabels(containerLabels);
                    }

                    foreach (var data in (configuration.ExpectedLabels ?? []).Select(value => value.Split('=', count: 2)))
                    {
                        if (!containerLabels.TryGetValue(data[0], out string? value))
                        {
                            clientLogger?.DockerContainerDoesNotContainExpectedLabel(data[0]);
                            return default;
                        }

                        // The expected label can be specified in the format name=value. In such cases, in
                        // addition to checking whether the label is present in the container, we need to
                        // check whether the value of the container's label matches the expected value.
                        if (data.Length == 2 && data[1] != value)
                        {
                            clientLogger?.DockerContainerDoesNotContainExpectedLabel(data[0], data[1]);
                            return default;
                        }
                    }

                    return Convert(container);
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    clientLogger?.DockerContainerNotFound();
                    return default;
                }
            }
        }

        public async IAsyncEnumerable<DockerContainerEvent> MonitorAsync(
            DateTime since, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var request = CreateRequest(HttpMethod.Get, "events").AddQueryParameters(new()
            {
                { "since", ((DateTimeOffset)since).ToUnixTimeSeconds().ToString() }
            });

            AddContainerFilters(request, new()
            {
                { "type", _supportedEventTypes },
                { "label", configuration.ExpectedLabels }
            });

            // The Docker API server will hold connections and when events occur, send a JSON string
            // for each event. There is always a line break at the end of a JSON string.
            await foreach (var content in request.GetStreamAsync(cancellationToken))
            {
                using var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                var containerEvent = await JsonSerializer.DeserializeAsync(
                    contentStream, JsonSerializerGeneratedContext.Default.DockerEventResponse, cancellationToken);

                if (containerEvent != default && _supportedEventTypes.Contains(containerEvent.Type))
                {
                    if (containerEvent.Action.StartsWith("exec_"))
                    {
                        continue;
                    }

                    clientLogger?.DockerSentEventMessage(content);

                    yield return new DockerContainerEvent
                    {
                        EventAction = containerEvent.Action,
                        EventType = containerEvent.Type,
                        ContainerId = containerEvent.Actor.Attributes.ContainerId ?? containerEvent.Actor.Id,
                    };
                }
            }
        }

        private static void AddContainerFilters(HttpRequest request, Dictionary<string, string[]?> containerFilters)
        {
            if (containerFilters.Count > 0)
            {
                // Before creating the request with resource filters, we will discard any filters
                // that have no values ​​and serialize the remaining resource filters to JSON format.
                // If for example the resource filters contain two labels, Docker will only return
                // containers that have both labels, not just one of them.
                containerFilters = containerFilters.Where(data => data.Value?.Length > 0).ToDictionary();
                if (containerFilters.Count > 0)
                {
                    var serializedFilters = JsonSerializer.Serialize(
                        containerFilters!, JsonSerializerGeneratedContext.Default.ContainerFilters);

                    request.AddQueryParameters(new() { { "filters", serializedFilters } });
                }
            }
        }

        private static DockerContainer Convert(BaseDockerResponse response)
        {
            var containerNetworks = response.NetworkSettings.Networks.Select(network =>
            {
                return new KeyValuePair<string, IPAddress?>(
                    network.Key,

                    // The Docker API always sends a valid IP address or a null/empty string. So we
                    // only check if there is a value in the property and parse the IP address.
                    network.Value.IPAddress?.Length > 0 ? IPAddress.Parse(network.Value.IPAddress!) : default);
            });

            var containerLabels = response switch
            {
                DockerResponse dockerResponse => dockerResponse.Labels,
                InspectedDockerResponse inspectedResponse => inspectedResponse.Configuration.Labels,
                _ => default
            };

            return new DockerContainer
            {
                Id = response.Id,
                Labels = containerLabels ?? new Dictionary<string, string>(),
                Networks = containerNetworks.ToDictionary()
            };
        }
    }
}
