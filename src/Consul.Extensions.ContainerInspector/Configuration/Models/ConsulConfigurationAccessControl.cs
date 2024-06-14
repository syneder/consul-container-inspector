namespace Consul.Extensions.ContainerInspector.Configuration.Models
{
    public class ConsulConfigurationAccessControl
    {
        [ConfigurationKeyName("tokens")]
        public ConsulConfigurationTokens? Tokens { get; set; }
    }
}
