using Consul.Extensions.ContainerInspector.Core.Configuration.Models;
using Microsoft.Extensions.Configuration;

namespace Consul.Extensions.ContainerInspector.Core.Configuration
{
    public class DockerInspectorConfigurationProvider : ConfigurationProviderBase
    {
        public DockerInspectorConfigurationProvider(string configurationSection) : base(configurationSection)
        {
            EnvironmentVariablesMap.Add(
                "DOCKER_CONTAINER_LABELS_SERVICE_NAME", ConfigurationPath.Combine(
                    nameof(DockerInspectorConfiguration.Labels),
                    nameof(DockerInspectorLabelConfiguration.ServiceLabel)));
        }
    }
}
