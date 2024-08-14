using System.Text.Json.Serialization;

namespace Consul.Extensions.ContainerInspector.Core.Models
{
    /// <summary>
    /// Describes the AWS credentials.
    /// </summary>
    public class ContainerCredentials
    {
        /// <summary>
        /// Gets or sets the access key identifier.
        /// </summary>
        [JsonPropertyName("AccessKeyId")]
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the access key secret.
        /// </summary>
        [JsonPropertyName("SecretAccessKey")]
        public required string Secret { get; set; }

        /// <summary>
        /// Gets or sets the security token.
        /// </summary>
        public required string Token { get; set; }

        /// <summary>
        /// Gets or sets the expiration date for these credentials.
        /// </summary>
        public required DateTime Expiration { get; set; }
    }
}
