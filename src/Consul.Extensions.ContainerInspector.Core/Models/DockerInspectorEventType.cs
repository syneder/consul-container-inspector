namespace Consul.Extensions.ContainerInspector.Core.Models
{
    /// <summary>
    /// Enum of possible Docker inspector events.
    /// </summary>
    public enum DockerInspectorEventType
    {
        /// <summary>
        /// The Docker inspector detected and inspected a Docker container.
        /// </summary>
        ContainerDetected = 1,

        /// <summary>
        /// The Docker inspector detected changes in the Docker container's networks.
        /// </summary>
        ContainerNetworksUpdated,

        /// <summary>
        /// The Docker inspector detected a Docker container pause event.
        /// </summary>
        ContainerPaused,

        /// <summary>
        /// The Docker inspector detected a Docker container unpause event.
        /// </summary>
        ContainerUnpaused,

        /// <summary>
        /// The Docker inspector detected a Docker container destruction event or the Docker
        /// container was not accepted by the inspector.
        /// </summary>
        ContainerDisposed,

        /// <summary>
        /// The Docker inspector detected that a Docker container has become healthy.
        /// </summary>
        ContainerHealthy,

        /// <summary>
        /// The Docker inspector detected that a Docker container has become unhealthy.
        /// </summary>
        ContainerUnhealthy,

        /// <summary>
        /// The Docker inspector has completed inspecting existing running Docker containers and
        /// has started monitoring and inspecting Docker events.
        /// </summary>
        ContainersInspectionCompleted,
    }
}
