using Consul.Extensions.ContainerInspector.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace Consul.Extensions.ContainerInspector.Core.Extensions
{
    /// <summary>
    /// Extension methods for registering custom <see cref="ConfigurationProvider" />.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Default configuration section where the read Docker configurations will be located.
        /// </summary>
        public const string DockerConfigurationSection = "Docker";

        public static IConfigurationBuilder AddDockerConfiguration(this IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.Add(new DockerConfigurationSource(DockerConfigurationSection));
        }
    }
}
