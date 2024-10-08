﻿namespace Consul.Extensions.ContainerInspector.Core.Models
{
    /// <summary>
    /// Describes the AWS ECS task.
    /// </summary>
    public class AmazonTask
    {
        /// <summary>
        /// Gets or sets the task ARN.
        /// </summary>
        public required AmazonTaskArn TaskArn { get; set; }

        /// <summary>
        /// Gets or sets the name of the group to which this task belongs.
        /// </summary>
        public required string Group { get; set; }
    }
}
