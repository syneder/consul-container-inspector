using Consul.Extensions.ContainerInspector.Configuration;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for registering <see cref="ConsulConfigurationProvider"/> with <see cref="IConfigurationBuilder"/>.
    /// </summary>
    public static class ConsulConfigurationExtensions
    {
        /// <summary>
        /// Default path to Consul configuration files.
        /// </summary>
        public const string ConfigurationFilePath = "/consul/config";

        /// <summary>
        /// Default configuration section where the read configurations will be located.
        /// </summary>
        public const string ConfigurationSection = "Consul";

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from Consul configuration files
        /// or environment variables.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddConsulConfiguration(this IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.Add(new ConsulConfigurationSource(ConfigurationFilePath, ConfigurationSection));
        }
    }
}
