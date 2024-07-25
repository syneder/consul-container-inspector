using Consul.Extensions.ContainerInspector.Configurations.Models;
using Consul.Extensions.ContainerInspector.Core;
using Consul.Extensions.ContainerInspector.Core.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding services to an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the <see cref="IHttpClientFactory" /> and related services to the
        /// <see cref="IServiceCollection" /> and configures <see cref="HttpClient" /> for the Amazon,
        /// Consul and Docker clients.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection ConfigureHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient(nameof(IConsulClient))
                .ConfigureHttpClient(ConsulClient.ConfigureHttpClient).ConfigureUnixSocket(serviceProvider =>
                {
                    var configuration = serviceProvider.GetRequiredService<ConsulConfiguration>();
                    return configuration.AddressBinding.SocketPath ?? "/consul/run/consul.sock";
                });

            services.AddHttpClient(nameof(IDockerClient)).ConfigureUnixSocket(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<DockerConfiguration>();
                return configuration.SocketPath ?? "/var/run/docker.sock";
            });

            services.AddHttpClient(nameof(IAmazonClient));
            return services;
        }

        /// <summary>
        /// Adds implementations of core services and related services to the <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IAmazonClient, AmazonClient>();
            services.TryAddTransient<IConsulClient, ConsulClient>();
            services.TryAddTransient<IDockerClient, DockerClient>();
            services.TryAddTransient<IDockerInspector, DockerInspector>();

            return services.ConfigureHttpClients();
        }
    }
}
