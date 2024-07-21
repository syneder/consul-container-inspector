using Consul.Extensions.ContainerInspector.Core.Models;

namespace Consul.Extensions.ContainerInspector.Core
{
    public interface IDockerInspector
    {
        /// <summary>
        /// Monitors state change events for Docker containers and the networks used by those
        /// containers, and then performs inspection.
        /// </summary>
        /// <returns>An infinite <see cref="IAsyncEnumerable{DockerInspectorEvent}" /> of events that
        /// can only be interrupted by <paramref name="cancellationToken" />.</returns>
        IAsyncEnumerable<DockerInspectorEvent> InspectAsync(CancellationToken cancellationToken);
    }
}
