namespace Consul.Extensions.ContainerInspector.Configuration.Models
{
    /// <summary>
    /// Describes the Consul agent configuration.
    /// </summary>
    public class ConsulConfiguration
    {
        [ConfigurationKeyName("node_name")]
        public string? Node { get; set; }

        [ConfigurationKeyName("acl")]
        public ConsulConfigurationAccessControl? AccessControlList { get; set; }
    }
}
