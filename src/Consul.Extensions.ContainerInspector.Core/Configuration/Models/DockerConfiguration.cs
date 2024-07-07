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
    }
}
