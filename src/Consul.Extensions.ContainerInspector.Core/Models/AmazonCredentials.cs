namespace Consul.Extensions.ContainerInspector.Core.Models
{
    /// <summary>
    /// Describes the AWS credentials.
    /// </summary>
    public class AmazonCredentials
    {
        /// <summary>
        /// Gets or sets the API access token.
        /// </summary>
        public required string Token { get; set; }

        /// <summary>
        /// Gets or sets the expiration date for these credentials.
        /// </summary>
        public required DateTime Expiration { get; set; }
    }
}
