namespace Consul.Extensions.ContainerInspector.Configuration.Models
{
    public class ConsulConfigurationTokens
    {
        [ConfigurationKeyName("agent")]
        public string? Agent { get; set; }
    }
}
