namespace Consul.Extensions.ContainerInspector.Configuration
{
    /// <summary>
    /// Represents AWS managed instance registration as an <see cref="IConfigurationSource"/>.
    /// </summary>
    public class ManagedInstanceRegistrationSource(string registrationFilePath, string configurationSection) : IConfigurationSource
    {
        /// <summary>
        /// Builds the <see cref="ManagedInstanceRegistrationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="ManagedInstanceRegistrationProvider"/>.</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ManagedInstanceRegistrationProvider(registrationFilePath, configurationSection);
        }
    }
}
