using Microsoft.Extensions.Configuration;
using System.Net;

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
        public IPAddress? AdvertiseAddress { get; set; }

        [ConfigurationKeyName("acl")]
        public AccessControlConfiguration AccessControlList { get; set; } = new();

        [ConfigurationKeyName("addresses")]
        public AddressBindingConfiguration AddressBinding { get; set; } = new();

        public class AccessControlConfiguration
        {
            internal Dictionary<string, string> Tokens { get; set; } = [];

            /// <summary>
            /// Gets the Consul agent token to manage service registration.
            /// </summary>
            public string? Token => Tokens.GetValueOrDefault("inspector") ?? Tokens.GetValueOrDefault("agent");
        }

        public class AddressBindingConfiguration
        {
            [ConfigurationKeyName("http")]
            internal string? HttpListenerAddresses { get; set; }

            /// <summary>
            /// Gets the Consul unix socket path.
            /// </summary>
            public string? SocketPath { get; set; }
        }
    }
}
