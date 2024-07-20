using Consul.Extensions.ContainerInspector.Configurations.Models;
using Microsoft.Extensions.Configuration;

namespace Consul.Extensions.ContainerInspector.Configurations
{
    /// <summary>
    /// Provides Docker inspector configuration key/values from environment variables.
    /// </summary>
    public class DockerInspectorConfigurationProvider(string configurationSection) : BaseConfigurationProvider(configurationSection)
    {
        protected override IDictionary<string, string> EnvsMapper => new Dictionary<string, string>
        {
            {
                "DOCKER_CONTAINER_LABELS_SERVICE_NAME", ConfigurationPath.Combine(
                    nameof(DockerInspectorConfiguration.Labels),
                    nameof(DockerInspectorConfiguration.LabelConfiguration.ServiceLabel))
            },
            {
                "DOCKER_CONTAINER_LABELS_SERVICE_HEALTH_NAME", ConfigurationPath.Combine(
                    nameof(DockerInspectorConfiguration.Labels),
                    nameof(DockerInspectorConfiguration.LabelConfiguration.ServiceHealthLabel))
            },
            {
                "DOCKER_CONTAINER_LABELS_SERVICE_HEALTH_INTERVAL_NAME", ConfigurationPath.Combine(
                    nameof(DockerInspectorConfiguration.Labels),
                    nameof(DockerInspectorConfiguration.LabelConfiguration.ServiceHealthIntervalLabel))
            },
            {
                "DOCKER_CONTAINER_LABELS_SERVICE_HEALTH_TIMEOUT_NAME", ConfigurationPath.Combine(
                    nameof(DockerInspectorConfiguration.Labels),
                    nameof(DockerInspectorConfiguration.LabelConfiguration.ServiceHealthTimeoutLabel))
            },
        };
    }
}
