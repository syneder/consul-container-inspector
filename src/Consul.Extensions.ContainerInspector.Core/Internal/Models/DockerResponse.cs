namespace Consul.Extensions.ContainerInspector.Core.Internal.Models
{
    /// <summary>
    /// Describes a Docker container obtained using the container list method.
    /// </summary>
    internal class DockerResponse : BaseDockerResponse
    {
        /// <summary>
        /// Gets or sets the state of the Docker container.
        /// </summary>
        public required string State { get; set; }

        /// <summary>
        /// Gets or sets the status of the Docker container.
        /// </summary>
        public required string Status { get; set; }

        /// <summary>
        /// Gets or sets the Docker container labels.
        /// </summary>
        public required IDictionary<string, string> Labels { get; set; }

        /// <summary>
        /// Parses the <see cref="Status" /> and returns the Docker container status.
        /// </summary>
        public string? ParseStatus()
        {
            var separatorIndex = Status.IndexOf('(');
            if (separatorIndex > 0)
            {
                var endSeparatorIndex = Status.IndexOf(')', separatorIndex);
                if (endSeparatorIndex > separatorIndex + 1)
                {
                    return Status[(separatorIndex + 1) .. endSeparatorIndex].ToLowerInvariant();
                }
            }

            return default;
        }
    }
}
