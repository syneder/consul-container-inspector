namespace Consul.Extensions.ContainerInspector.Configuration.Models
{
    public class ConsulConfiguration
    {
        [ConfigurationKeyName("node_name")]
        public string? Node { get; set; }

        [ConfigurationKeyName("acl")]
        public ConsulConfigurationAccessControl? AccessControlList { get; set; }
    }
}
