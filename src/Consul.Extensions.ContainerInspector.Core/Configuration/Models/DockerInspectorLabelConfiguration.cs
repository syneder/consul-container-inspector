namespace Consul.Extensions.ContainerInspector.Core.Configuration.Models
{
    /// <summary>
    /// Enumerates Docker container label names.
    /// </summary>
    public class DockerInspectorLabelConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the Docker container label that contains the name of the service.
        /// </summary>
        public string ServiceLabel { get; set; } = "consul.inspector.service.name";
    }
}
