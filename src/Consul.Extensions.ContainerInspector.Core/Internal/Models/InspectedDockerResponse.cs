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
        /// Describes the configuration of a Docker container.
        /// </summary>
        public class ContainerConfiguration
        {
            /// <summary>
            /// Gets or sets the Docker container labels.
            /// </summary>
            public required IDictionary<string, string> Labels { get; set; }
        }
    }
}
