using Microsoft.Extensions.Configuration;

namespace Consul.Extensions.ContainerInspector.Configurations.Models
{
    /// <summary>
    /// Describes the Consul agent configuration.
    /// </summary>
    public partial class ConsulConfiguration
    {
        /// <summary>
        /// Gets or sets the IP address of the Consul agent.
        /// </summary>
        [ConfigurationKeyName("advertise_addr")]
        public string? AdvertiseAddress { get; set; }

        [ConfigurationKeyName("acl")]
        public AccessControlConfiguration AccessControlList { get; } = new();

        [ConfigurationKeyName("addresses")]
        public AddressBindingConfiguration AddressBinding { get; } = new();

        public class AccessControlConfiguration
        {
            public Dictionary<string, string> Tokens { get; } = [];

            /// <summary>
            /// Gets the Consul agent token to manage service registration.
            /// </summary>
            public string? Token => Tokens.GetValueOrDefault("inspector") ?? Tokens.GetValueOrDefault("agent");
        }

        public class AddressBindingConfiguration
        {
            [ConfigurationKeyName("http")]
            public string? HttpListenerAddresses { get; set; }

            /// <summary>
            /// Gets the Consul unix socket path.
            /// </summary>
            public string? SocketPath { get; set; }
        }
    }
}
