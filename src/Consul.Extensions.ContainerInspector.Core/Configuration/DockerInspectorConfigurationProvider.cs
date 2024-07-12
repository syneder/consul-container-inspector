using Consul.Extensions.ContainerInspector.Core.Configuration.Models;
using Microsoft.Extensions.Configuration;

namespace Consul.Extensions.ContainerInspector.Core.Configuration
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
                    nameof(DockerInspectorLabelConfiguration.ServiceLabel))
            }
        };
    }
}
