using Consul.Extensions.ContainerInspector.Core.Models;
using Microsoft.Extensions.Logging;

namespace Consul.Extensions.ContainerInspector.Core.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IAmazonClient" />.
    /// </summary>
    internal class AmazonClient(
        IHttpClientFactory clientFactory,
        ILogger<IDockerClient>? clientLogger) : IAmazonClient
    {
        public async Task<AmazonCredentials?> GetCredentialsAsync(CancellationToken cancellationToken)
        {
            return default;
        }

        public async Task<IEnumerable<AmazonTask>> DescribeTasksAsync(
            IEnumerable<string> arns, CancellationToken cancellationToken)
        {
            // To reduce the number of API requests to AWS, group the found resource ARNs by
            // region and then by cluster. In a standard configuration, only one group will exist,
            // since the same container instance can only be connected to one cluster. But this
            // rule can be broken by using a certain configuration.

            //foreach (var regionGroup in resourceArns.GroupBy(resourceArn => resourceArn?.Region))
            //{
            //    foreach (var clusterGroup in regionGroup.GroupBy(descriptor => resourceArn?.Cluster))
            //    {
            //        // Cluster = clusterGroup.Key,
            //        // Tasks = clusterGroup.Select(resourceArn => resourceArn?.Arn)
            //    }
            //}

            return [];
        }
    }
}
