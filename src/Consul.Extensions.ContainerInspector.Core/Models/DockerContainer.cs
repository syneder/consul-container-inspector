using System.Net;

namespace Consul.Extensions.ContainerInspector.Core.Models
{
    /// <summary>
    /// Describes the Docker container.
    /// </summary>
    public class DockerContainer
    {
        /// <summary>
        /// Gets identifier of Docker container.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Gets the list of networks connected to Docker container and their corresponding IP addresses.
        /// </summary>
        public required IDictionary<string, IPAddress?> Networks { get; init; }

        /// <summary>
        /// Gets the labels of Docker container.
        /// </summary>
        public required IDictionary<string, string> Labels { get; set; }

        /// <summary>
        /// Gets or sets whether the Docker container is healthy.
        /// </summary>
        public bool IsHealthy { get; set; }

        /// <summary>
        /// Gets or sets whether the Docker container is suspended.
        /// </summary>
        public bool IsSuspended { get; set; }
    }
}
