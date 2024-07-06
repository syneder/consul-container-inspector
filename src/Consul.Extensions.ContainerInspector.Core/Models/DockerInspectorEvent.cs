namespace Consul.Extensions.ContainerInspector.Core.Models
{
    /// <summary>
    /// Describes the Docker inspector event.
    /// </summary>
    public class DockerInspectorEvent(DockerInspectorEventType eventType)
    {
        /// <summary>
        /// Gets the Docker inspector event type.
        /// </summary>
        public DockerInspectorEventType Type { get; } = eventType;

        /// <summary>
        /// Gets the service name that the Docker inspector defined for the specified Docker container.
        /// </summary>
        public string? ServiceName { get; init; }

        /// <summary>
        /// Gets information about the Docker container for which the Docker inspector event occurred.
        /// </summary>
        public DockerInspectorEventDescriptor? Descriptor { get; init; }
    }
}
