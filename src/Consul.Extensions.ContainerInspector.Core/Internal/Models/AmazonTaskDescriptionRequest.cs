namespace Consul.Extensions.ContainerInspector.Core.Internal.Models
{
    /// <summary>
    /// Describes a request content model for describing AWS ECS ​​tasks.
    /// </summary>
    internal class AmazonTaskDescriptionRequest
    {
        /// <summary>
        /// Gets or sets the cluster name.
        /// </summary>
        public required string Cluster { get; set; }

        /// <summary>
        /// Gets or sets an enumeration of the ARNs of the tasks that need to be described.
        /// </summary>
        public required IEnumerable<string> Tasks { get; set; }
    }
}
