using System.Text.Json.Serialization;

namespace Consul.Extensions.ContainerInspector.Core.Internal.Models
{
    /// <summary>
    /// Describes the Docker event about a container or network.
    /// </summary>
    internal class DockerEventResponse
    {
        /// <summary>
        /// Gets or sets the Docker event type.
        /// </summary>
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the Docker event action.
        /// </summary>
        public required string Action { get; set; }

        /// <summary>
        /// Gets or sets the object that is emitted the event.
        /// </summary>
        public required EventActor Actor { get; set; }

        public class EventActor
        {
            /// <summary>
            /// Gets or sets the identifier of the object that is emitted the event.
            /// </summary>
            public required string Id { get; set; }

            /// <summary>
            /// Gets or sets the attributes of the object that is emitted the event.
            /// </summary>
            public required ActorAttributes Attributes { get; set; }
        }

        public class ActorAttributes
        {
            /// <summary>
            /// Gets or sets the Docker container identifier.
            /// </summary>
            [JsonPropertyName("container")]
            public string? ContainerId { get; set; }
        }
    }
}
