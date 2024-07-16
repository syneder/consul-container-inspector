using Consul.Extensions.ContainerInspector.Core.Configuration.Models;
using Consul.Extensions.ContainerInspector.Core.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Consul.Extensions.ContainerInspector.Core.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IConsul" />.
    /// </summary>
    internal class Consul(
        IOptions<ConsulConfiguration> options, IHttpClientFactory clientFactory) : IConsul
    {
        public async Task<IEnumerable<ServiceRegistration>> GetServicesAsync(CancellationToken cancellationToken)
        {
            using var requestClient = clientFactory.CreateClient(nameof(IConsul));
            using var requestMessage = CreateRequestMessage(HttpMethod.Get, "agent/services");

            var response = await ExecuteRequestAsync<IDictionary<string, ServiceRegistration>>(
                requestClient, requestMessage, cancellationToken);

            return response?.Values ?? [];
        }

        public async Task RegisterServiceAsync(ServiceRegistration service, CancellationToken cancellationToken)
        {
            using var requestClient = clientFactory.CreateClient(nameof(IConsul));
            using var requestMessage = CreateRequestMessage(HttpMethod.Put, "agent/service/register");
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(service));

            using (await ExecuteRequestAsync(requestClient, requestMessage, cancellationToken)) { }
        }

        public async Task UnregisterServiceAsync(string serviceId, CancellationToken cancellationToken)
        {
            using var requestClient = clientFactory.CreateClient(nameof(IConsul));
            using var requestMessage = CreateRequestMessage(HttpMethod.Put, $"agent/service/deregister/{serviceId}");

            using (await ExecuteRequestAsync(requestClient, requestMessage, cancellationToken)) { }
        }

        /// <summary>
        /// Creates a <see cref="HttpRequestMessage" /> and adds an authentication header if the
        /// token is set in the configuration.
        /// </summary>
        private HttpRequestMessage CreateRequestMessage(HttpMethod method, string resourceUri)
        {
            var requestMessage = new HttpRequestMessage(method, $"/v1/{resourceUri}");
            if (options.Value.AccessControlList?.Tokens?.Agent?.Length > 0)
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", options.Value.AccessControlList.Tokens.Agent);
            }

            return requestMessage;
        }

        /// <summary>
        /// Sends the request, waits to read the response headers, and ensures that the response
        /// status code is successful.
        /// </summary>
        private static async Task<HttpResponseMessage> ExecuteRequestAsync(
            HttpClient requestClient, HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var responseMessage = await requestClient.SendAsync(
                requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            return responseMessage.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Executes the request, reads the contents of the response, and deserializes it into an object.
        /// </summary>
        private static async Task<T?> ExecuteRequestAsync<T>(
            HttpClient requestClient, HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            using var responseMessage = await ExecuteRequestAsync(requestClient, requestMessage, cancellationToken);
            using var contentStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);

            return await JsonSerializer.DeserializeAsync<T>(contentStream, cancellationToken: cancellationToken);
        }
    }
}
