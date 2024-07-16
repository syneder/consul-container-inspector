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

        /// <summary>
        /// Default configuration section where the read Docker inspector configurations will be located.
        /// </summary>
        public const string DockerInspectorConfigurationSection = "Core";

        /// <summary>
        /// Default configuration section where the read Consul configurations will be located.
        /// </summary>
        public const string ConsulConfigurationSection = "Consul";

        /// <summary>
        /// Default path to Consul configuration files.
        /// </summary>
        public const string ConsulConfigurationFilePath = "/consul/config";

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from Consul
        /// configuration files or environment variables.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddConsulConfiguration(this IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.AddConfigurationProvider(
                () => new ConsulConfigurationProvider(ConsulConfigurationFilePath, ConsulConfigurationSection));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads Docker configuration values from
        /// environment variables.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddDockerConfiguration(this IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.AddConfigurationProvider(
                () => new DockerConfigurationProvider(DockerConfigurationSection));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads Docker inspector configuration
        /// values from environment variables.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddDockerInspectorConfiguration(this IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.AddConfigurationProvider(
                () => new DockerInspectorConfigurationProvider(DockerInspectorConfigurationSection));
        }

        public static IConfigurationBuilder AddConfigurationProvider(
            this IConfigurationBuilder configurationBuilder, Func<IConfigurationProvider> providerFactory)
        {
            return configurationBuilder.Add(new ConfigurationSource(providerFactory));
        }

        private class ConfigurationSource(Func<IConfigurationProvider> providerFactory) : IConfigurationSource
        {
            public IConfigurationProvider Build(IConfigurationBuilder builder) => providerFactory();
        }
    }
}
