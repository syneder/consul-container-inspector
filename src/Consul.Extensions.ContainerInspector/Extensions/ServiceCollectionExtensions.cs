using Consul.Extensions.ContainerInspector.Internal;
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
        public static IServiceCollection AddDockerClient(this IServiceCollection services)
        {
            var builder = services.AddHttpClient(nameof(Docker), client =>
            {
                client.BaseAddress = new Uri("http://localhost");
            });

            builder.ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new SocketsHttpHandler { ConnectCallback = ConnectAsync };
            });

            return services.AddTransient<Docker>();
        }

        /// <summary>
        /// Connects to Docker using Unix socket and returns a <see cref="NetworkStream" />.
        /// </summary>
        private static async ValueTask<Stream> ConnectAsync(
            SocketsHttpConnectionContext context, CancellationToken cancellationToken)
        {
            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            var endpoint = new UnixDomainSocketEndPoint("/var/run/docker.sock");

            await socket.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
            return new NetworkStream(socket, ownsSocket: false);
        }
    }
}
