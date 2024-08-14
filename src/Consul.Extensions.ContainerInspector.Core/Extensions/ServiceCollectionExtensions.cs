using Consul.Extensions.ContainerInspector.Configurations.Models;
using Consul.Extensions.ContainerInspector.Core;
using Consul.Extensions.ContainerInspector.Core.Internal;
using Consul.Extensions.ContainerInspector.Core.Internal.Converters;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json;

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
            services
                .AddHttpClient(nameof(IConsulClient))
                .ConfigureHttpClient(ConsulClient.ConfigureHttpClient)
                .ConfigureHttpLogging()
                .ConfigureUnixSocket("consul", serviceProvider =>
                {
                    var configuration = serviceProvider.GetRequiredService<ConsulConfiguration>();
                    return configuration.AddressBinding.SocketPath ?? "/consul/run/consul.sock";
                });

            services
                .AddHttpClient(nameof(IDockerClient))
                .ConfigureHttpLogging()
                .ConfigureUnixSocket("docker", serviceProvider =>
                {
                    var configuration = serviceProvider.GetRequiredService<DockerConfiguration>();
                    return configuration.SocketPath ?? "/var/run/docker.sock";
                });

            services
                .AddHttpClient(nameof(ContainerCredentialsProvider))
                .ConfigureHttpClient(ContainerCredentialsProvider.ConfigureHttpClient)
                .ConfigureHttpLogging();

            services.AddHttpClient(nameof(IAmazonClient)).ConfigureHttpLogging();
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

            services.AddSingleton(serviceProvider =>
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    TypeInfoResolver = JsonSerializerGeneratedContext.Default,
                };

                options.Converters.Add(new AmazonTaskArnConverter());
                return options;
            });

            return services.ConfigureHttpClients();
        }
    }
}
