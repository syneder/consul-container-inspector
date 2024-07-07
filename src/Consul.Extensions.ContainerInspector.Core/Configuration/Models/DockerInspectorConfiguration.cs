namespace Consul.Extensions.ContainerInspector.Core.Configuration.Models
{
    /// <summary>
    /// Describe the Docker inspector configuration.
    /// </summary>
    public class DockerInspectorConfiguration
    {
        /// <summary>
        /// Gets or sets the container label names.
        /// </summary>
        public DockerInspectorLabelConfiguration Labels { get; set; } = new();
    }
}
