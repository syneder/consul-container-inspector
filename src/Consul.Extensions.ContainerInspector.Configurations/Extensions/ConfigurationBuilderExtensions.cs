using Consul.Extensions.ContainerInspector.Configurations;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for registering custom <see cref="ConfigurationProvider" />.
    /// </summary>
    public static class ConfigurationBuilderExtensions
    {
        private static readonly IEnumerable<Func<IConfigurationProvider>> _configurationProviderFactories =
        [
            () => new ConsulConfigurationProvider(ConsulConfigurationFilePath, ConsulConfigurationSection),
            () => new DockerConfigurationProvider(DockerConfigurationSection),
            () => new DockerInspectorConfigurationProvider(DockerInspectorConfigurationSection)
        ];

        /// <summary>
        /// Default configuration section where the read Consul configurations will be located.
        /// </summary>
        public const string ConsulConfigurationSection = "consul";

        /// <summary>
        /// Default configuration section where the read Docker configurations will be located.
        /// </summary>
        public const string DockerConfigurationSection = "docker";

        /// <summary>
        /// Default configuration section where the read Docker inspector configurations will be located.
        /// </summary>
        public const string DockerInspectorConfigurationSection = "core";

        /// <summary>
        /// Default path to Consul configuration files.
        /// </summary>
        public const string ConsulConfigurationFilePath = "/consul/config";

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads Consul, Docker and Docker
        /// inspector configuration values from configuration files and environment variables.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddServiceConfigurations(this IConfigurationBuilder configurationBuilder)
        {
            foreach (var providerFactory in _configurationProviderFactories)
            {
                configurationBuilder = configurationBuilder.Add(new ConfigurationSource(providerFactory));
            }

            return configurationBuilder;
        }

        private class ConfigurationSource(Func<IConfigurationProvider> providerFactory) : IConfigurationSource
        {
            public IConfigurationProvider Build(IConfigurationBuilder builder) => providerFactory();
        }
    }
}
