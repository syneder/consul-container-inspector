using Consul.Extensions.ContainerInspector.Configurations.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding services to an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers service configuration instances as a singleton that sets a value by binding
        /// the configuration in a specific section with a configuration type.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to register with.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection BindServiceConfigurations(this IServiceCollection services)
        {
            services.AddOptions<ConsulConfiguration>()
                .BindConfiguration(ConfigurationBuilderExtensions.ConsulConfigurationSection);

            services.AddOptions<DockerConfiguration>()
                .BindConfiguration(ConfigurationBuilderExtensions.DockerConfigurationSection);

            services.AddOptions<DockerInspectorConfiguration>()
                .BindConfiguration(ConfigurationBuilderExtensions.DockerInspectorConfigurationSection);

            services.AddOptions<ManagedInstanceRegistration>()
                .BindConfiguration(ConfigurationBuilderExtensions.ManagedInstanceConfigurationSection);

            services.PostConfigure<ConsulConfiguration>(configuration =>
            {
                var addressBinding = configuration.AddressBinding;
                if (addressBinding.HttpListenerAddresses?.Length > 0 && addressBinding.SocketPath?.Length == 0)
                {
                    var addresses = addressBinding.HttpListenerAddresses.Split();
                    var socketAddress = addresses.FirstOrDefault(address => address.StartsWith("unix://"));
                    addressBinding.SocketPath = socketAddress?["unix://".Length..];
                }
            });

            services.TryAddSingleton(GetRequiredOptions<ConsulConfiguration>);
            services.TryAddSingleton(GetRequiredOptions<DockerConfiguration>);
            services.TryAddSingleton(GetRequiredOptions<DockerInspectorConfiguration>);
            services.TryAddSingleton(GetRequiredOptions<ManagedInstanceRegistration>);

            return services;
        }

        private static T GetRequiredOptions<T>(IServiceProvider serviceProvider) where T : class
        {
            return serviceProvider.GetRequiredService<IOptions<T>>().Value;
        }
    }
}
