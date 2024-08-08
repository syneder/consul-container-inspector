using Consul.Extensions.ContainerInspector.Core.Models;

namespace Consul.Extensions.ContainerInspector.Core
{
    public interface IAmazonClient
    {
        /// <summary>
        /// Sends an API request to the AWS Agent to obtain credentials, or obtains credentials
        /// from cache if cached credentials have not expired.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        /// <returns>
        /// <see cref="Task{AmazonCredentials?}" /> that completes with obtained or cached AWS
        /// credentials, or null if the credentials cannot be obtained.
        /// </returns>
        Task<ContainerCredentials?> GetCredentialsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Sends an API request to AWS to describe ECS tasks with the specified ARNs.
        /// </summary>
        /// <param name="arns">Enumerate of task ARNs that need to be described.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
        /// <returns><see cref="Task{IEnumerable{AmazonTask}}" /> that completes with enumerate
        /// of the existed described tasks.</returns>
        Task<IEnumerable<AmazonTask>> DescribeTasksAsync(IEnumerable<AmazonTaskArn> arns, CancellationToken cancellationToken);
    }
}
