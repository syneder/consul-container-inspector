namespace Consul.Extensions.ContainerInspector.Core.Models
{
    /// <summary>
    /// Describes the Docker container in which the Docker event occurred.
    /// </summary>
    /// <remarks>
    /// It can contain complete information about the Docker container or only its identifier (for
    /// example, if the Docker container destruction event occurred and there is no information about it).
    /// </remarks>
    public class DockerInspectorEventDescriptor
    {
        private string _containerId = string.Empty;

        /// <summary>
        /// Gets the identifier of the Docker container in which the event occurred.
        /// </summary>
        public string ContainerId
        {
            get => Container?.Id ?? _containerId;
            init => _containerId = value;
        }

        /// <summary>
        /// Gets the complete information of the Docker container in which the event occurred.
        /// </summary>
        public DockerContainer? Container { get; init; }

        public DockerInspectorEventDescriptor(DockerContainer container) => Container = container;
        public DockerInspectorEventDescriptor(string containerId) => ContainerId = containerId;
    }
}
