using Consul.Extensions.ContainerInspector.Core.Configuration.Models;

namespace Consul.Extensions.ContainerInspector.Core.Configuration
{
    public class DockerConfigurationProvider : ConfigurationProviderBase
    {
        public const string ConfigurationEnvironmentName = "DOCKER_SOCKET_PATH";

        public DockerConfigurationProvider(string configurationSection) : base(configurationSection)
        {
            EnvironmentVariablesMap.Add(ConfigurationEnvironmentName, nameof(DockerConfiguration.SocketPath));
        }
    }
}
