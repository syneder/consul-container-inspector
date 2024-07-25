using System.Net;
using System.Text.Json.Serialization;

namespace Consul.Extensions.ContainerInspector.Core.Models
{
    /// <summary>
    /// Describes service registration in Consul cluster.
    /// </summary>
    public class ServiceRegistration(string name)
    {
        /// <summary>
        /// Gets or sets the identifier for the service.
        /// </summary>
        /// <remarks>
        /// Services on the same node must have unique identifiers.
        /// </remarks>
        [JsonPropertyName("ID")]
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// Gets or sets string value that specifies a service IP address or hostname.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Gets or sets a list of string values as service-level labels.
        /// </summary>
        /// <remarks>
        /// Tag values are opaque to Consul. Use valid DNS labels for service definition identifiers
        /// for compatibility with external DNS.
        /// </remarks>
        public string[] Tags { get; set; } = [];

        /// <summary>
        /// Gets or sets the meta field contains custom key-value pairs that associate semantic
        /// metadata with the service.
        /// </summary>
        [JsonPropertyName("Meta")]
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        public ServiceRegistration(string name, IPAddress? address) : this(name) => Address = address?.ToString();
    }
}
