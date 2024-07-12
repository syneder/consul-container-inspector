namespace Consul.Extensions.ContainerInspector.Core.Configuration.Models
{
    /// <summary>
    /// Describes the Docker client configuration.
    /// </summary>
    public class DockerConfiguration
    {
        /// <summary>
        /// Gets or sets the Docker unix socket path.
        /// </summary>
        public string SocketPath { get; set; } = "/var/run/docker.sock";

        /// <summary>
        /// Gets or sets the labels that should be contained in containers returned by the Docker client.
        /// </summary>
        public string[]? ExpectedLabels { get; set; }
    }
}
