namespace Consul.Extensions.ContainerInspector.Core.Internal.Models
{
    /// <summary>
    /// Describes a Docker container obtained using the container list method.
    /// </summary>
    internal class DockerResponse : BaseDockerResponse
    {
        /// <summary>
        /// Gets or sets the state of the Docker container.
        /// </summary>
        public required string State { get; set; }

        /// <summary>
        /// Gets or sets the Docker container labels.
        /// </summary>
        public required Dictionary<string, string> Labels { get; set; }
    }
}
