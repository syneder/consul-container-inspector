using Consul.Extensions.ContainerInspector.Core.Internal;
using Consul.Extensions.ContainerInspector.Core.Models;
using Microsoft.Extensions.Logging;

namespace Consul.Extensions.ContainerInspector.Extensions
{
    internal static partial class LoggerExtensions
    {
        [LoggerMessage(100, LogLevel.Trace, "Detected new Docker {eventType} event ({eventAction}). Event content: {content}")]
        public static partial void DetectedDockerEvent(this ILogger serviceLogger, string eventType, string eventAction, string content);

        [LoggerMessage(101, LogLevel.Trace, "Detected changes in network configurations in the Docker container with identifier {containerId}. Previous networks: {networks}. Current networks: {currentNetworks}.")]
        public static partial void DetectedNetworkChanges(this ILogger serviceLogger, string containerId, string networks, string currentNetworks);

        [LoggerMessage(102, LogLevel.Trace, "The {containerLabel} label with a value '{value}' was found in Docker container identifier {containerId}. This value will be used as the service name.")]
        public static partial void DetectedServiceLabel(this ILogger serviceLogger, string containerId, string containerLabel, string value);

        [LoggerMessage(200, LogLevel.Debug, "The Docker container with identifier {containerId} does not exist or was deleted before the request completed.")]
        public static partial void DockerContainerNotFound(this ILogger serviceLogger, string containerId);

        [LoggerMessage(201, LogLevel.Debug, "The Docker container with identifier {containerId} does not contain the '{containerLabel}' label.")]
        public static partial void DockerContainerExpectedLabelMissing(this ILogger serviceLogger, string containerId, string containerLabel);

        [LoggerMessage(202, LogLevel.Debug, "The Docker container with identifier {containerId} does not contain the '{containerLabel}' label with the value '{value}'.")]
        public static partial void DockerContainerExpectedLabelMissing(this ILogger serviceLogger, string containerId, string containerLabel, string value);

        [LoggerMessage(203, LogLevel.Debug, "Detected and ignored new Docker {eventType} event ({eventAction}) about Docker container with identifier {containerId}.")]
        public static partial void DockerEventIgnored(this ILogger serviceLogger, string eventType, string eventAction, string containerId);

        [LoggerMessage(204, LogLevel.Debug, "After inspecting the Docker container with identifier {containerId}, the service name '{serviceName}' was defined for this container.")]
        public static partial void ServiceNameDefined(this ILogger serviceLogger, string containerId, string serviceName);

        [LoggerMessage(205, LogLevel.Debug, "Detected changes in network configurations in the Docker container with identifier {containerId}.")]
        public static partial void DetectedNetworkChanges(this ILogger serviceLogger, string containerId);

        [LoggerMessage(206, LogLevel.Debug, "Detected resource ARN '{resourceArn}' in Docker container with identifier {containerId}.")]
        public static partial void DetectedResourceArn(this ILogger serviceLogger, string containerId, string resourceArn);

        [LoggerMessage(400, LogLevel.Warning, "It is impossible to inspect the Docker container with identifier {containerId} because it has been destroyed and there is no information about it in the cache.")]
        public static partial void CannotInspectDisposedDockerContainer(this ILogger serviceLogger, string containerId);

        [LoggerMessage(401, LogLevel.Warning, "It is impossible to inspect the Docker container with identifier {containerId} because the container does not exist in Docker.")]
        public static partial void CannotInspectNotExistedDockerContainer(this ILogger serviceLogger, string containerId);

        [LoggerMessage(500, LogLevel.Error, $"The same resource ARN '{{resourceArn}}' was detected for multiple Docker containers. The container label {DockerInspector.ResourceArnLabel} may have been added manually.")]
        public static partial void DuplicateResourceArn(this ILogger serviceLogger, string resourceArn);

        [LoggerMessage(501, LogLevel.Error, "Failed to parse resource ARN for Docker container with identifier {containerId}.")]
        public static partial void ResourceArnInvalid(this ILogger serviceLogger, string containerId);

        public static string SerializeContainerNetworks(DockerContainer container)
        {
            return string.Join(", ", container.Networks.Select(network =>
            {
                return network.Value == default ? network.Key : $"{network.Key} ({network.Value})";
            }));
        }
    }
}
