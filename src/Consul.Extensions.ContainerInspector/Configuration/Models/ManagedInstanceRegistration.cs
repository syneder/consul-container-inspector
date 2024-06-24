using System.Text.Json.Serialization;

namespace Consul.Extensions.ContainerInspector.Configuration.Models
{
    public class ManagedInstanceRegistration
    {
        [JsonPropertyName("ManagedInstanceID")]
        public string? InstanceId { get; set; }
    }
}
