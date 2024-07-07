using Microsoft.Extensions.Configuration;

namespace Consul.Extensions.ContainerInspector.Core.Configuration
{
    /// <summary>
    /// Represents Docker environment variables as an <see cref="IConfigurationSource"/>.
    /// </summary>
    public class DockerConfigurationSource(string configurationSection) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new DockerConfigurationProvider(configurationSection);
        }
    }
}
