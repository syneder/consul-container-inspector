using Consul.Extensions.ContainerInspector.Core.Configuration.Models;
using Microsoft.Extensions.Configuration;
using System.Collections;

namespace Consul.Extensions.ContainerInspector.Core.Configuration
{
    public class DockerConfigurationProvider(string configurationSection) : ConfigurationProvider
    {
        public const string ConfigurationEnvironmentName = "DOCKER_SOCKETPATH";

        /// <summary>
        /// Loads the Docker environment variable.
        /// </summary>
        public override void Load()
        {
            LoadFromEnvironmentVariables(Environment.GetEnvironmentVariables());
        }

        private void LoadFromEnvironmentVariables(IDictionary envs)
        {
            if (envs.Contains(ConfigurationEnvironmentName))
            {
                var value = envs[ConfigurationEnvironmentName] as string;
                if (value?.Length > 0)
                {
                    var configurationPath = ConfigurationPath.Combine(
                        configurationSection, nameof(DockerConfiguration.SocketPath));

                    Data.TryAdd(configurationPath, value);
                }
            }
        }
    }
}
