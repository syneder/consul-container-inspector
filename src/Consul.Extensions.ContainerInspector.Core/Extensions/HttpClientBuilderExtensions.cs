using Consul.Extensions.ContainerInspector.Core.Internal.Logging;
using Consul.Extensions.ContainerInspector.Extensions;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring an <see cref="IHttpClientBuilder" />.
    /// </summary>
    internal static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Configures the <see cref="HttpClient" /> to use a unix socket.
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder" />.</param>
        /// <param name="socketPathProvider">A delegate used to provide the path to a unix socket.</param>
        public static IHttpClientBuilder ConfigureUnixSocket(
            this IHttpClientBuilder builder, string hostname, Func<IServiceProvider, string> socketPathProvider)
        {
            return builder
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri($"http://{hostname}");
                })
                .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                {
                    var socketPath = socketPathProvider(serviceProvider);
                    if (!File.Exists(socketPath))
                    {
                        var serviceLoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                        if (serviceLoggerFactory != default)
                        {
                            var serviceLogger = CreateHttpLogger(serviceLoggerFactory);
                            serviceLogger.UnixSocketDoesNotExist(socketPath);
                        }
                    }

                    var socketEndpoint = new UnixDomainSocketEndPoint(socketPath);
                    return CreateSocketHttpHandler(socketEndpoint);
                });
        }

        /// <summary>
        /// Configures logging of the lifecycle for an HTTP request.
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder" />.</param>
        public static IHttpClientBuilder ConfigureHttpLogging(this IHttpClientBuilder builder)
        {
            return builder.RemoveAllLoggers().ConfigureAdditionalHttpMessageHandlers((messageHandlers, serviceProvider) =>
            {
                var serviceLoggerFactory = serviceProvider.GetService<ILoggerFactory>();
                if (serviceLoggerFactory != default)
                {
                    var serviceLogger = CreateHttpLogger(serviceLoggerFactory);
                    messageHandlers.Insert(0, new LoggingScopeMessageHandler(serviceLogger));

                    // We want this handler to be last so we can log details about the request after
                    // service discovery and security happen.
                    messageHandlers.Add(new LoggingMessageHandler(serviceLogger));
                }
            });
        }

        private static SocketsHttpHandler CreateSocketHttpHandler(UnixDomainSocketEndPoint endpoint)
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

        private static ILogger CreateHttpLogger(ILoggerFactory serviceLoggerFactory)
        {
            return serviceLoggerFactory.CreateLogger("Consul.Extensions.ContainerInspector.Core.HttpClient");
        }
    }
}
