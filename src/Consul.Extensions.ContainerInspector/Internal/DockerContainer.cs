using System.Net;

namespace Consul.Extensions.ContainerInspector.Internal
{
    public class DockerContainer
    {
        public required string Id { get; init; }
        public required string State { get; init; }
        public required IDictionary<string, IPAddress?> Networks { get; init; }
        public required IDictionary<string, string> Labels { get; init; }
    }
}
