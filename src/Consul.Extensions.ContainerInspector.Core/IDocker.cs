using Consul.Extensions.ContainerInspector.Core.Models;

namespace Consul.Extensions.ContainerInspector.Core
{
    public interface IDocker
    {
        public static readonly Version DockerVersion = new("1.43");

        /// <summary>
        /// Returns running containers and information needed to register services in Consul.
        /// </summary>
        /// <returns><see cref="Task{IEnumerable{DockerContainer}}" /> that completes with enumerate
        /// of the container data that contain information for registering services in Consul.</returns>
        Task<IEnumerable<DockerContainer>> GetContainersAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Returns information about the container with the specified <paramref name="containerId"/>,
        /// needed to register service in Consul.
        /// </summary>
        /// <returns><see cref="Task{DockerContainer?}" /> that completes with container data that
        /// contain information for registering service in Consul.</returns>
        Task<DockerContainer?> GetContainerAsync(string containerId, CancellationToken cancellationToken);

        /// <summary>
        /// Monitors state change events for containers and the networks used by those containers.
        /// </summary>
        /// <returns>An infinite <see cref="IAsyncEnumerable{DockerContainerEvent}"/> of events that
        /// can only be interrupted by <paramref name="cancellationToken"/>.</returns>
        IAsyncEnumerable<DockerContainerEvent> MonitorAsync(DateTime since, CancellationToken cancellationToken);
    }
}
