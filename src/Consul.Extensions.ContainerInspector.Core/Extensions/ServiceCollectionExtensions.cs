using Consul.Extensions.ContainerInspector.Configurations.Models;
using Consul.Extensions.ContainerInspector.Core;
using Consul.Extensions.ContainerInspector.Core.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding services to an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private static readonly Dictionary<string, Func<IServiceProvider, EndPoint>> _endpointFactories = [];

        static ServiceCollectionExtensions()
        {
            _endpointFactories.Add(nameof(IConsulClient), serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<ConsulConfiguration>();
                return new UnixDomainSocketEndPoint(
                    configuration.AddressBinding.SocketPath ?? "/consul/run/consul.sock");
            });

            _endpointFactories.Add(nameof(IDockerClient), serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<DockerConfiguration>();
                return new UnixDomainSocketEndPoint(configuration.SocketPath ?? "/var/run/docker.sock");
            });
        }

        /// <summary>
        /// Adds the <see cref="IHttpClientFactory" /> and related services to the
        /// <see cref="IServiceCollection" /> and configures <see cref="HttpClient" /> for the Amazon,
        /// Consul and Docker clients.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection ConfigureHttpClients(this IServiceCollection services)
        {
            foreach (var (name, endpointFactory) in _endpointFactories)
            {
                services.ConfigureHttpClient(name).ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                {
                    return CreateSocketHttpHandler(endpointFactory(serviceProvider));
                });
            }

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

        private static IHttpClientBuilder ConfigureHttpClient(this IServiceCollection services, string name)
        {
            return services.AddHttpClient(name).ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("http://localhost");
            });
        }

        private static SocketsHttpHandler CreateSocketHttpHandler(EndPoint endpoint)
        {
            return new SocketsHttpHandler
            {
                ConnectCallback = async (_, cancellationToken) =>
                {
                    var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                    await socket.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);

                    return new NetworkStream(socket, ownsSocket: false);
                }
            };
        }
    }
}
