namespace Consul.Extensions.ContainerInspector.Configurations.Models
{
    /// <summary>
    /// Describe the Docker inspector configuration.
    /// </summary>
    public class DockerInspectorConfiguration
    {
        /// <summary>
        /// Gets or sets the container label names.
        /// </summary>
        public LabelConfiguration Labels { get; set; } = new();

        /// <summary>
        /// Enumerates Docker container label names.
        /// </summary>
        public class LabelConfiguration
        {
            /// <summary>
            /// Gets or sets the name of the Docker container label that contains the name of the service.
            /// </summary>
            public string ServiceLabel { get; set; } = "consul.inspector.service.name";

            /// <summary>
            /// Gets or sets the name of the Docker container label that contains the HTTP, TCP, or UDP
            /// endpoint to which the Consul agent will send health check requests.
            /// </summary>
            public string ServiceHealthLabel { get; set; } = "consul.inspector.service.health";

            /// <summary>
            /// Gets or sets the name of the Docker container label that contains the interval at
            /// which Consul agent should send requests to check the health of the service.
            /// </summary>
            public string ServiceHealthIntervalLabel { get; set; } = "consul.inspector.service.health.interval";

            /// <summary>
            /// Gets or sets the name of the Docker container label that contains the timeout that
            /// Consul agent should wait for a response to a health check request.
            /// </summary>
            public string ServiceHealthTimeoutLabel { get; set; } = "consul.inspector.service.health.timeout";
        }
    }
}
