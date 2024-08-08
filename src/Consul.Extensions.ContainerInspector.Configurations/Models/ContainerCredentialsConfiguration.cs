namespace Consul.Extensions.ContainerInspector.Configurations.Models
{
    /// <summary>
    /// Describes the container credentials configuration.
    /// </summary>
    public class ContainerCredentialsConfiguration
    {
        /// <summary>
        /// Gets or sets the HTTP URL endpoint to use when requesting container credentials.
        /// </summary>
        public string? ProviderUri { get; set; }

        /// <summary>
        /// Gets or sets the maximum lifetime of obtained credentials.
        /// </summary>
        public TimeSpan Lifetime { get; set; } = TimeSpan.FromMinutes(10);
    }
}
