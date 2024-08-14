using Consul.Extensions.ContainerInspector.Configurations.Models;
using Consul.Extensions.ContainerInspector.Core.Models;
using System.Text.Json;

namespace Consul.Extensions.ContainerInspector.Core.Internal
{
    /// <summary>
    /// Provides methods for obtaining Amazon credentials.
    /// </summary>
    internal class ContainerCredentialsProvider(
        IHttpClientFactory clientFactory,
        ContainerCredentialsConfiguration configuration,
        JsonSerializerOptions serializerOptions) : BaseClient(nameof(ContainerCredentialsProvider), clientFactory)
    {
        /// <summary>
        /// Tries to obtain Amazon credentials, or returns null if not possible.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        /// <returns><see cref="Task{AmazonCredentials?}" /> that completes with Amazon credentials,
        /// or null if credentials cannot be obtained.</returns>
        public async Task<ContainerCredentials?> GetCredentialsAsync(CancellationToken cancellationToken)
        {
            if ((configuration.ProviderUri?.Length ?? 0) == 0)
            {
                return default;
            }

            using var request = CreateRequest(HttpMethod.Get, configuration.ProviderUri!, serializerOptions);
            return UpdateExpiration(await request.ExecuteRequestAsync<ContainerCredentials>(cancellationToken));
        }

        /// <summary>
        /// Configures the <see cref="HttpClient" /> for requests to Consul.
        /// </summary>
        public static void ConfigureHttpClient(HttpClient client)
        {
            client.BaseAddress = new Uri("http://169.254.170.2");
        }

        private ContainerCredentials? UpdateExpiration(ContainerCredentials? credentials)
        {
            if (credentials != default)
            {
                var expiration = DateTime.Now.Add(configuration.Lifetime);
                if (credentials.Expiration > expiration)
                {
                    credentials.Expiration = expiration;
                }
            }

            return credentials;
        }
    }
}
