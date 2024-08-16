using Consul.Extensions.ContainerInspector.Configurations.Models;
using Consul.Extensions.ContainerInspector.Core.Internal.Models;
using Consul.Extensions.ContainerInspector.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Consul.Extensions.ContainerInspector.Core.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IConsulClient" />.
    /// </summary>
    internal class ConsulClient(IHttpClientFactory clientFactory, JsonSerializerOptions serializerOptions)
        : BaseClient(nameof(IConsulClient), clientFactory), IConsulClient
    {
        protected override string BaseResourceUri => "v1";

        public async Task<IEnumerable<ServiceRegistration>> GetServicesAsync(CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Get, "agent/services", serializerOptions);
            var response = await request.ExecuteRequestAsync<IDictionary<string, ServiceRegistrationResponse>>(cancellationToken);

            return response?.Values ?? [];
        }

        public async Task RegisterServiceAsync(ServiceRegistration service, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Put, "agent/service/register", serializerOptions);
            using var requestContent = JsonContent.Create(service, serializerOptions.GetTypeInfo(service.GetType()));

            request.Message.Content = requestContent;
            using (await request.ExecuteRequestAsync(cancellationToken)) { }
        }

        public async Task UnregisterServiceAsync(string serviceId, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Put, $"agent/service/deregister/{serviceId}", serializerOptions);
            using (await request.ExecuteRequestAsync(cancellationToken)) { }
        }

        /// <summary>
        /// Configures the <see cref="HttpClient" /> for requests to Consul.
        /// </summary>
        /// <param name="serviceProvider">An instance of the root service provider.</param>
        internal static void ConfigureHttpClient(IServiceProvider serviceProvider, HttpClient client)
        {
            var configuration = serviceProvider.GetRequiredService<ConsulConfiguration>();
            if (configuration.AccessControlList.Token?.Length > 0)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer", configuration.AccessControlList.Token);
            }
        }
    }
}
