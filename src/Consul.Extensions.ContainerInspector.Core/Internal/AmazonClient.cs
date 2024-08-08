using Consul.Extensions.ContainerInspector.Configurations.Models;
using Consul.Extensions.ContainerInspector.Core.Models;

namespace Consul.Extensions.ContainerInspector.Core.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IAmazonClient" />.
    /// </summary>
    internal class AmazonClient(
        IHttpClientFactory clientFactory,
        ContainerCredentialsConfiguration configuration) : BaseClient(nameof(IAmazonClient), clientFactory), IAmazonClient
    {
        private ContainerCredentials? _credentials = default;
        private ContainerCredentialsProvider? _credentialsProvider = new(clientFactory, configuration);

        public async Task<ContainerCredentials?> GetCredentialsAsync(CancellationToken cancellationToken)
        {
            if (_credentialsProvider == default)
            {
                return default;
            }

            if (_credentials == default || DateTime.Now > _credentials.Expiration)
            {
                if ((_credentials = await _credentialsProvider.GetCredentialsAsync(cancellationToken)) == default)
                {
                    // The only reason a provider can return null is if it is not possible to obtain
                    // credentials. So if the provider returns null, remove the provider so it does
                    // not try to obtain credentials next time and returns null immediately.
                    _credentialsProvider = default;
                }
            }

            return _credentials;
        }

        public async Task<IEnumerable<AmazonTask>> DescribeTasksAsync(
            IEnumerable<AmazonTaskArn> arns, CancellationToken cancellationToken)
        {
            // To reduce the number of API requests to AWS, group the task ARNs by region and then
            // by cluster. In a standard configuration, only one group will exist, since the same
            // container instance can only be connected to one cluster. But this rule can be broken
            // by using a certain configuration.
            foreach (var regionGroup in arns.GroupBy(arn => arn.Region))
            {
                foreach (var clusterGroup in regionGroup.GroupBy(arn => arn.Cluster))
                {
                    using var request = CreateRequest(HttpMethod.Get, "");

                    // Endpoint = regionGroup....
                    // Cluster = clusterGroup.Key,
                    // Tasks = clusterGroup.Select(resourceArn => resourceArn?.Arn)
                }
            }

            return [];
        }
    }
}
