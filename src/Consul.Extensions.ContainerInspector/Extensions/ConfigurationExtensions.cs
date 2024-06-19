using Consul.Extensions.ContainerInspector.Configuration;

namespace Consul.Extensions.ContainerInspector.Extensions
{
    /// <summary>
    /// Extension methods for registering custom <see cref="ConfigurationProvider" />.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Default path to Consul configuration files.
        /// </summary>
        public const string ConsulConfigurationFilePath = "/consul/config";

        /// <summary>
        /// Default configuration section where the read Consul configurations will be located.
        /// </summary>
        public const string ConsulConfigurationSection = "Consul";

        /// <summary>
        /// Default path to AWS managed instance registration.
        /// </summary>
        public const string ManagedInstanceRegistrationFilePath = "/amazon/ssm/registration";

        /// <summary>
        /// Default configuration section where the read AWS managed instance registration will be located.
        /// </summary>
        public static readonly string ManagedInstanceConfigurationSection = ConfigurationPath.Combine("Amazon", "SSM");

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from Consul configuration files
        /// or environment variables.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddConsulConfiguration(this IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.Add(
                new ConsulConfigurationSource(ConsulConfigurationFilePath, ConsulConfigurationSection));
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from AWS managed instance
        /// registration.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddManagedInstanceRegistration(this IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.Add(
                new ManagedInstanceRegistrationSource(
                    ManagedInstanceRegistrationFilePath, ManagedInstanceConfigurationSection));
        }
    }
}
