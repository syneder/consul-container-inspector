using System.Text.Json.Serialization;

namespace Consul.Extensions.ContainerInspector.Core.Internal.Models
{
    /// <summary>
    /// Describes a Docker container obtained using the get container by identifier method.
    /// </summary>
    internal class InspectedDockerResponse : BaseDockerResponse
    {
        /// <summary>
        /// Gets or sets the Docker container configuration.
        /// </summary>
        [JsonPropertyName("Config")]
        public required ContainerConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the Docker container state.
        /// </summary>
        public required ContainerState State { get; set; }

        /// <summary>
        /// Describes the configuration of a Docker container.
        /// </summary>
        public class ContainerConfiguration
        {
            /// <summary>
            /// Gets or sets the Docker container labels.
            /// </summary>
            public required IDictionary<string, string> Labels { get; set; }
        }

        /// <summary>
        /// Describes the state of a Docker container.
        /// </summary>
        public class ContainerState
        {
            /// <summary>
            /// Gets or sets whether a Docker container is paused.
            /// </summary>
            public bool Paused { get; set; }

            /// <summary>
            /// Gets or sets the health status of a Docker container.
            /// </summary>
            public ContainerHealth? Health { get; set; }
        }

        /// <summary>
        /// Describes the health status of a Docker container.
        /// </summary>
        public class ContainerHealth
        {
            /// <summary>
            /// Gets or sets the Docker container health status.
            /// </summary>
            public required string Status { get; set; }
        }
    }
}
