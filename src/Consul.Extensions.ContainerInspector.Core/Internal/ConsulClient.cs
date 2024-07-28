using Consul.Extensions.ContainerInspector.Configurations.Models;
using Consul.Extensions.ContainerInspector.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Consul.Extensions.ContainerInspector.Core.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IConsulClient" />.
    /// </summary>
    internal class ConsulClient(IHttpClientFactory clientFactory)
        : BaseClient(nameof(IConsulClient), clientFactory), IConsulClient
    {
        protected override string BaseResourceURI => "v1";

        public async Task<IEnumerable<ServiceRegistration>> GetServicesAsync(CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Get, "agent/services");
            var response = await request.ExecuteRequestAsync(
                JsonSerializerGeneratedContext.Default.ServiceRegistrationResponseDictionary, cancellationToken);

            return response?.Values ?? [];
        }

        public async Task RegisterServiceAsync(ServiceRegistration service, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Put, "agent/service/register");
            using var requestContent = JsonContent.Create(
                service, JsonSerializerGeneratedContext.Default.ServiceRegistration);

            request.RequestMessage.Content = requestContent;
            using (await request.ExecuteRequestAsync(cancellationToken)) { }
        }

        public async Task UnregisterServiceAsync(string serviceId, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Put, $"agent/service/deregister/{serviceId}");
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
