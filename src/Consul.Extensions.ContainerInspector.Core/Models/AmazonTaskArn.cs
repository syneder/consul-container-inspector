﻿namespace Consul.Extensions.ContainerInspector.Core.Models
{
    public record AmazonTaskArn(string Arn, string ResourceId, string Region, string Cluster)
    {
        /// <summary>
        /// The name of the Docker container label containing the ARN of the task that manages
        /// the specific Docker container.
        /// </summary>
        public const string ContainerLabel = "com.amazonaws.ecs.task-arn";

        public static AmazonTaskArn? GetTaskArn(DockerContainer container)
        {
            if (!container.Labels.TryGetValue(ContainerLabel, out var arn))
            {
                return default;
            }

            // In the current implementation, ARN creation is only allowed for ECS tasks. The ARN
            // has the following format, but not all components of the ARN are required and will be
            // available once created: arn:{partition}:{service}:{region}:{account-id}:ecs/{cluster}/{resourceId}
            if (arn.Split(':') is [_, _, _, var region, _, var resource] &&
                resource.Split('/') is [_, var cluster, var resourceId])
            {
                return new AmazonTaskArn(arn, resourceId, region, cluster);
            }

            throw new TaskArnParseException(arn);
        }
    }

    public class TaskArnParseException(string arn) : Exception
    {
        public string TaskArn => arn;
    }
}
