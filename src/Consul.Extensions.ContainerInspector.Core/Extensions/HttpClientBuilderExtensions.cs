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
        /// <param name="socketProvider">A delegate used to provide the path to a unix socket.</param>
        /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
        public static IHttpClientBuilder ConfigureUnixSocket(
            this IHttpClientBuilder builder, Func<IServiceProvider, string> socketProvider)
        {
            return builder
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("http://localhost"))
                .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                {
                    var socketEndpoint = new UnixDomainSocketEndPoint(socketProvider(serviceProvider));
                    return CreateSocketHttpHandler(socketEndpoint);
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
    }
}
