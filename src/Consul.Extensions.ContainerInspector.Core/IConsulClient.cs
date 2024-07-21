using Consul.Extensions.ContainerInspector.Core.Models;

namespace Consul.Extensions.ContainerInspector.Core
{
    public interface IConsulClient
    {
        /// <summary>
        /// Sends an API request to the Consul agent to receive an enumerable of registered services.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        /// <returns><see cref="Task{IEnumerable{ServiceRegistration}}" /> that completes with enumerate
        /// of the registered services in Consul.</returns>
        Task<IEnumerable<ServiceRegistration>> GetServicesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Sends an API request to the Consul agent to register <paramref name="service" />.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        Task RegisterServiceAsync(ServiceRegistration service, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an API request to the Consul agent to unregister existed service.
        /// </summary>
        /// <param name="serviceId">
        /// The identifier of the existing service that should be unregistered.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        Task UnregisterServiceAsync(string serviceId, CancellationToken cancellationToken);
    }
}
