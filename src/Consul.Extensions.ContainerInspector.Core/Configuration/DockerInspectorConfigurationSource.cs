using Microsoft.Extensions.Configuration;

namespace Consul.Extensions.ContainerInspector.Core.Configuration
{
    /// <summary>
    /// Represents Docker inspector environment variables as an <see cref="IConfigurationSource"/>.
    /// </summary>
    public class DockerInspectorConfigurationSource(string configurationSection) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new DockerInspectorConfigurationProvider(configurationSection);
        }
    }
}
