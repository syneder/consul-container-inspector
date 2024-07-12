using Consul.Extensions.ContainerInspector.Core;
using Consul.Extensions.ContainerInspector.Core.Configuration.Models;
using Consul.Extensions.ContainerInspector.Core.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Net.Sockets;

using ConfigurationExtensions = Consul.Extensions.ContainerInspector.Core.Extensions.ConfigurationExtensions;

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
            services.ConfigureDockerHttpClient();

            services.Configure<DockerConfiguration>(
                configuration.GetSection(ConfigurationExtensions.DockerConfigurationSection));

            services.TryAddTransient<IDocker, Docker>();
            return services;
        }

        public static IServiceCollection AddDockerInspector(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDocker(configuration);

            services.Configure<DockerInspectorConfiguration>(
                configuration.GetSection(ConfigurationExtensions.DockerInspectorConfigurationSection));

            services.TryAddTransient<IDockerInspector, DockerInspector>();
            return services;
        }

        private static IHttpClientBuilder ConfigureDockerHttpClient(this IServiceCollection services)
        {
            return services.AddHttpClient(nameof(IDocker))
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("http://localhost"))
                .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                {
                    var configuration = serviceProvider.GetRequiredService<IOptions<DockerConfiguration>>().Value;

                    return new SocketsHttpHandler
                    {
                        ConnectCallback = (_, cancellationToken) =>
                        {
                            var endpoint = new UnixDomainSocketEndPoint(configuration.SocketPath);
                            return Docker.ConnectAsync(endpoint, cancellationToken);
                        }
                    };
                });
        }
    }
}
