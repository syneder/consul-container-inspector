using System.Text.Json;

namespace Consul.Extensions.ContainerInspector.Core.Models
{
    public sealed record AmazonTaskArn(string EncodedArn, string ResourceId, string Region, string Cluster)
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

            return ParseTaskArn(arn);
        }

        public static AmazonTaskArn ParseTaskArn(string arn)
        {
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

        public static AmazonTaskArn? ParseTaskArn(Utf8JsonReader reader)
        {
            var stringValue = reader.GetString();
            return stringValue == default ? default : ParseTaskArn(stringValue);
        }

        public bool Equals(AmazonTaskArn? instance)
        {
            if (instance == default)
            {
                return default;
            }

            return instance.EncodedArn == EncodedArn || (
                instance.ResourceId == ResourceId &&
                instance.Region == Region &&
                instance.Cluster == Cluster);
        }

        public override int GetHashCode()
        {
            return ResourceId.GetHashCode() ^ Region.GetHashCode() ^ Cluster.GetHashCode();
        }
    }

    public class TaskArnParseException(string arn) : Exception
    {
        public string EncodedArn => arn;
    }
}
