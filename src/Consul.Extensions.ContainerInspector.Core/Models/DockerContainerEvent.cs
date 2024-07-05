namespace Consul.Extensions.ContainerInspector.Core.Models
{
    public class DockerContainerEvent
    {
        public required string EventAction { get; init; }
        public required string EventType { get; init; }

        /// <summary>
        /// Gets the identifier of Docker container in which the event occurred.
        /// </summary>
        public required string ContainerId { get; init; }
    }
}
