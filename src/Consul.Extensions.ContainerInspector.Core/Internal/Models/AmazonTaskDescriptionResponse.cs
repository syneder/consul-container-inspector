using Consul.Extensions.ContainerInspector.Core.Models;

namespace Consul.Extensions.ContainerInspector.Core.Internal.Models
{
    /// <summary>
    /// Describes the content model of the response with the described AWS ECS tasks.
    /// </summary>
    internal class AmazonTaskDescriptionResponse
    {
        /// <summary>
        /// Gets or sets the described ECS tasks.
        /// </summary>
        public required IEnumerable<AmazonTask> Tasks { get; set; }
    }
}
