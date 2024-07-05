using Consul.Extensions.ContainerInspector.Core;
using Consul.Extensions.ContainerInspector.Core.Configuration.Models;
using Consul.Extensions.ContainerInspector.Core.Internal;
using Microsoft.Extensions.Options;
using System.Net.Sockets;

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
        public static IServiceCollection AddDocker(this IServiceCollection services)
        {
            services.AddHttpClient(nameof(Docker))
                .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                {
                    var options = serviceProvider.GetService<IOptions<DockerConfiguration>>()?.Value
                        ?? throw new InvalidOperationException("IOptions<DockerConfiguration> is not configured.");

                    return CreateMessageHandler(options);
                })
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("http://localhost"));

            return services.AddTransient<IDocker, Docker>();
        }

        private static SocketsHttpHandler CreateMessageHandler(DockerConfiguration configuration)
        {
            return new SocketsHttpHandler
            {
                ConnectCallback = (_, cancellationToken) =>
                {
                    var endpoint = new UnixDomainSocketEndPoint(configuration.SocketPath);
                    return Docker.ConnectAsync(endpoint, cancellationToken);
                }
            };
        }
    }
}
