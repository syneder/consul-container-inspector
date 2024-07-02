namespace Consul.Extensions.ContainerInspector.Internal
{
    public class DockerContainerEvent
    {
        public required string EventAction { get; init; }
        public required string EventType { get; init; }
        public required string ContainerId { get; init; }
    }
}
