using Consul.Extensions.ContainerInspector.Core.Models;
using Microsoft.Extensions.Logging;

namespace Consul.Extensions.ContainerInspector.Core.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IAmazonClient" />.
    /// </summary>
    internal class AmazonClient(
        IHttpClientFactory clientFactory,
        ILogger<IAmazonClient>? clientLogger) : IAmazonClient
    {
        public async Task<AmazonCredentials?> GetCredentialsAsync(CancellationToken cancellationToken)
        {
            return default;
        }

        public async Task<IEnumerable<AmazonTask>> DescribeTasksAsync(
            IEnumerable<AmazonTaskArn> arns, CancellationToken cancellationToken)
        {
            // To reduce the number of API requests to AWS, group the found resource ARNs by
            // region and then by cluster. In a standard configuration, only one group will exist,
            // since the same container instance can only be connected to one cluster. But this
            // rule can be broken by using a certain configuration.
            foreach (var regionGroup in arns.GroupBy(arn => arn.Region))
            {
                foreach (var clusterGroup in regionGroup.GroupBy(arn => arn.Cluster))
                {
                    // Cluster = clusterGroup.Key,
                    // Tasks = clusterGroup.Select(resourceArn => resourceArn?.Arn)
                }
            }

            return [];
        }
    }
}
