using Consul.Extensions.ContainerInspector.Configurations.Models;

namespace Consul.Extensions.ContainerInspector.Configurations
{
    /// <summary>
    /// Provides container credentials configuration key/values from environment variables.
    /// </summary>
    public class ContainerCredentialsConfigurationProvider(string configurationSection)
        : BaseConfigurationProvider(configurationSection)
    {
        protected override IDictionary<string, string> EnvsMapper => new Dictionary<string, string>()
        {
            { "AWS_CONTAINER_CREDENTIALS_RELATIVE_URI", nameof(ContainerCredentialsConfiguration.ProviderUri) },
        };
    }
}
