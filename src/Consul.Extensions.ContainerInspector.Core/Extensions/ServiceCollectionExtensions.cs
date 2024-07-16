using Consul.Extensions.ContainerInspector.Core;
using Consul.Extensions.ContainerInspector.Core.Configuration.Models;
using Consul.Extensions.ContainerInspector.Core.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;
using ConfigurationExtensions = Consul.Extensions.ContainerInspector.Core.Extensions.ConfigurationExtensions;
using Core = Consul.Extensions.ContainerInspector.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding services to an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the Docker client and the configuration to create an <see cref="HttpClient" />
        /// by <see cref="IHttpClientFactory"/>, that connects to Docker using Unix socket.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to register with.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDocker(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureHttpClient(nameof(IDocker)).ConfigurePrimaryHttpMessageHandler(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<DockerConfiguration>>();
                return CreateSocketHttpHandler(new UnixDomainSocketEndPoint(options.Value.SocketPath));
            });

            services.Configure<DockerConfiguration>(
                configuration.GetSection(ConfigurationExtensions.DockerConfigurationSection));

            services.TryAddTransient<IDocker, Docker>();
            return services;
        }

        /// <summary>
        /// Registers the Consul client and the configuration to create an <see cref="HttpClient" />
        /// by <see cref="IHttpClientFactory"/>, that connects to Consul using Unix socket.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to register with.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureHttpClient(nameof(IConsul)).ConfigurePrimaryHttpMessageHandler(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<ConsulConfiguration>>();
                return CreateSocketHttpHandler(new UnixDomainSocketEndPoint(options.Value.SocketPath));
            });

            services.Configure<ConsulConfiguration>(
                configuration.GetSection(ConfigurationExtensions.ConsulConfigurationSection));

            services.TryAddTransient<IConsul, Core.Internal.Consul>();
            return services;
        }

        /// <summary>
        /// Registers the Docker inspector and its configuration.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to register with.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDockerInspector(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDocker(configuration);

            services.Configure<DockerInspectorConfiguration>(
                configuration.GetSection(ConfigurationExtensions.DockerInspectorConfigurationSection));

            services.TryAddTransient<IDockerInspector, DockerInspector>();
            return services;
        }

        private static IHttpClientBuilder ConfigureHttpClient(this IServiceCollection services, string name)
        {
            return services.AddHttpClient(name).ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("http://localhost");
            });
        }

        private static SocketsHttpHandler CreateSocketHttpHandler(EndPoint socketEndpoint)
        {
            return new SocketsHttpHandler
            {
                ConnectCallback = (_, cancellationToken) =>
                {
                    return Docker.ConnectAsync(socketEndpoint, cancellationToken);
                }
            };
        }
    }
}
