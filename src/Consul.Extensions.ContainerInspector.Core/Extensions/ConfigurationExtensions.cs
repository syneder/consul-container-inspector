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

        public static IConfigurationBuilder AddDockerConfiguration(this IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.AddConfigurationProvider(
                () => new DockerConfigurationProvider(DockerConfigurationSection));
        }

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
