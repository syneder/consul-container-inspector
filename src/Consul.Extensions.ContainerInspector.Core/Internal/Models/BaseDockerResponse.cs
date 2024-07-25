namespace Consul.Extensions.ContainerInspector.Core.Internal.Models
{
    /// <summary>
    /// Describes the basic model for a successful Docker response about a container.
    /// </summary>
    internal class BaseDockerResponse
    {
        /// <summary>
        /// Gets or sets the Docker container identifier.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the Docker container network settings.
        /// </summary>
        public required ContainerNetworkSettings NetworkSettings { get; set; }

        /// <summary>
        /// Describes the Docker container network settings.
        /// </summary>
        public class ContainerNetworkSettings
        {
            /// <summary>
            /// Gets or sets the networks connected to the Docker container.
            /// </summary>
            public required IDictionary<string, ContainerNetwork> Networks { get; set; }
        }

        /// <summary>
        /// Describes the network connected to the Docker container.
        /// </summary>
        public class ContainerNetwork
        {
            /// <summary>
            /// Gets or sets the IP address assigned to the Docker container.
            /// </summary>
            public string? IPAddress { get; set; }
        }
    }
}
