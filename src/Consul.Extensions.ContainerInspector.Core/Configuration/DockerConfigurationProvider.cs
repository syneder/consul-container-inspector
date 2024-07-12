using Consul.Extensions.ContainerInspector.Core.Configuration.Models;

namespace Consul.Extensions.ContainerInspector.Core.Configuration
{
    /// <summary>
    /// Provides Docker configuration key/values from environment variables.
    /// </summary>
    public class DockerConfigurationProvider(string configurationSection) : BaseConfigurationProvider(configurationSection)
    {
        protected override IDictionary<string, string> EnvsMapper => new Dictionary<string, string>()
        {
            { "DOCKER_SOCKET_PATH", nameof(DockerConfiguration.SocketPath) },
            { "DOCKER_EXPECTED_CONTAINER_LABELS", $"[{nameof(DockerConfiguration.ExpectedLabels)}]" }
        };
    }
}
