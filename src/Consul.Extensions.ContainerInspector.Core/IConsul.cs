using Consul.Extensions.ContainerInspector.Core.Models;

namespace Consul.Extensions.ContainerInspector.Core
{
    public interface IConsul
    {
        /// <summary>
        /// Returns registered services in the Consul agent.
        /// </summary>
        Task<IEnumerable<ServiceRegistration>> GetServicesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Registers a new service in the Consul agent.
        /// </summary>
        Task RegisterServiceAsync(ServiceRegistration service, CancellationToken cancellationToken);

        /// <summary>
        /// Unregister existed service in the Consul agent.
        /// </summary>
        Task UnregisterServiceAsync(string serviceId, CancellationToken cancellationToken);
    }
}
