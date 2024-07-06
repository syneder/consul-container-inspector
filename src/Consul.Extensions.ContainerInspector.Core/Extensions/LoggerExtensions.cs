using Consul.Extensions.ContainerInspector.Core.Internal;
using Microsoft.Extensions.Logging;

namespace Consul.Extensions.ContainerInspector.Extensions
{
    public static partial class LoggerExtensions
    {
        [LoggerMessage(100, LogLevel.Debug, "Detected and ignored new {eventType} event ({eventAction}) about container with identifier {containerId}.")]
        public static partial void DockerEventIgnored(this ILogger serviceLogger, string eventType, string eventAction, string containerId);

        [LoggerMessage(300, LogLevel.Warning, "It is impossible to inspect the container with identifier {containerId} because it has been destroyed and there is no information about it in the cache.")]
        public static partial void CannotInspectDisposedDockerContainer(this ILogger serviceLogger, string containerId);

        [LoggerMessage(301, LogLevel.Warning, "It is impossible to inspect the container with identifier {containerId} because the container does not exist in Docker.")]
        public static partial void CannotInspectNotExistedDockerContainer(this ILogger serviceLogger, string containerId);

        [LoggerMessage(500, LogLevel.Error, $"The same resource ARN '{{resourceArn}}' was detected for multiple Docker containers. The container label {DockerInspector.ResourceArnLabel} may have been added manually.")]
        public static partial void DuplicateResourceArn(this ILogger serviceLogger, string resourceArn);

        [LoggerMessage(501, LogLevel.Error, "Failed to parse resource ARN for container with identifier {containerId}.")]
        public static partial void ResourceArnInvalid(this ILogger serviceLogger, string containerId);

    }
}
