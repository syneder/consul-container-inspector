using Microsoft.Extensions.Configuration;
using System.Net;

namespace Consul.Extensions.ContainerInspector.Core.Configuration.Models
{
    /// <summary>
    /// Describes the Consul agent configuration.
    /// </summary>
    public class ConsulConfiguration
    {
        /// <summary>
        /// Gets or sets the Consul unix socket path.
        /// </summary>
        public string SocketPath { get; set; } = "/consul/run/consul.sock";

        [ConfigurationKeyName("acl")]
        public ConsulConfigurationAccessControl? AccessControlList { get; set; }
    }
}
