namespace Consul.Extensions.ContainerInspector.Configuration
{
    /// <summary>
    /// Represents Consul configuration files or environment variables as an <see cref="IConfigurationSource"/>.
    /// </summary>
    public class ConsulConfigurationSource(string configurationFilePath, string configurationSection) : IConfigurationSource
    {
        /// <summary>
        /// Builds the <see cref="ConsulConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="ConsulConfigurationProvider"/>.</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConsulConfigurationProvider(configurationFilePath, configurationSection);
        }
    }
}
