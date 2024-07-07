using System.Text.Json.Serialization;

namespace Consul.Extensions.ContainerInspector.Configuration.Models
{
    /// <summary>
    /// Describes the registration of the current managed instance.
    /// </summary>
    public class ManagedInstanceRegistration
    {
        /// <summary>
        /// Gets or sets the identifier of the current managed instance.
        /// </summary>
        [JsonPropertyName("ManagedInstanceID")]
        public string? InstanceId { get; set; }
    }
}
