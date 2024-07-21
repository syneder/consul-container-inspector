using Consul.Extensions.ContainerInspector.Core.Internal;
using Consul.Extensions.ContainerInspector.Core.Models;
using Microsoft.Extensions.Logging;

namespace Consul.Extensions.ContainerInspector.Extensions
{
    public static partial class LoggerExtensions
    {
        [LoggerMessage(1, LogLevel.Trace, "The Docker client created request message (request URL: '{requestUri}')")]
        public static partial void DockerRequestMessageCreated(this ILogger serviceLogger, Uri? requestUri);

        [LoggerMessage(2, LogLevel.Trace, "The Consul client created request message (request URL: '{requestUri}', HTTP method: {method}, serialized content: {content})")]
        public static partial void ConsulRequestMessageCreated(this ILogger serviceLogger, string method, Uri? requestUri, string? content);

        [LoggerMessage(3, LogLevel.Trace, "The Consul client request message contains authorization token (request URL: '{requestUri}', HTTP method: {method})")]
        public static partial void ConsulRequestMessageContainsToken(this ILogger serviceLogger, string method, Uri? requestUri);

        [LoggerMessage(4, LogLevel.Trace, "The client sent an API request to Docker and received the response with 404 status code (request URL: '{requestUri}')")]
        public static partial void DockerReturnedNotFoundStatusCode(this ILogger serviceLogger, string? requestUri);

        [LoggerMessage(5, LogLevel.Trace, "The client sent an API request to Docker and received list of Docker containers (Docker containers count: {count})")]
        public static partial void DockerReturnedContainers(this ILogger serviceLogger, int count);

        [LoggerMessage(6, LogLevel.Trace, "Docker container with identifier {containerId} contains labels: {serializedContainerLabels}")]
        public static partial void DockerContainerContainsLabels(this ILogger serviceLogger, string containerId, string serializedContainerLabels);

        [LoggerMessage(7, LogLevel.Trace, "The Docker client received a message about a new Docker event (event content: {eventContent})")]
        public static partial void DockerSentEventMessage(this ILogger serviceLogger, string eventContent);

        [LoggerMessage(8, LogLevel.Trace, "The Docker inspector detected Docker container with identifier {containerId} (possible service name: '{serviceName}', container networks: {containerNetworks})")]
        public static partial void DockerInspectorDetectedDockerContainer(this ILogger serviceLogger, string? serviceName, string containerId, string? containerNetworks);

        [LoggerMessage(9, LogLevel.Trace, "Descriptor for the Docker container with identifier {containerId} was not found in the cache")]
        public static partial void DockerContainerDescriptorNotFoundInCache(this ILogger serviceLogger, string containerId);

        [LoggerMessage(10, LogLevel.Trace, "Detected changes in network configurations in the Docker container with identifier {containerId} (current networks: [{currentNetworks}], previous networks: [{networks}])")]
        public static partial void DockerContainerNetworkConfigurationChanged(this ILogger serviceLogger, string containerId, string? currentNetworks, string? networks);

        [LoggerMessage(11, LogLevel.Trace, "The Docker inspector defined the service name '{name}' using the Docker container label named '{containerLabel}' for Docker container with identifier {containerId}")]
        public static partial void DockerInspectorDefinedServiceName(this ILogger serviceLogger, string containerId, string containerLabel, string name);

        [LoggerMessage(12, LogLevel.Trace, "The Docker inspector defined the service name '{serviceName}' for the Docker container with identifier {containerId} using the name of the running task '{taskArn}' in the AWS ECS cluster named '{clusterName}'")]
        public static partial void DockerInspectorDefinedServiceName(this ILogger serviceLogger, string containerId, string clusterName, string serviceName, string taskArn);

        [LoggerMessage(100, LogLevel.Debug, "The Docker container with identifier {containerId} does not exist or was deleted before the API request completed")]
        public static partial void DockerContainerNotFound(this ILogger serviceLogger, string containerId);

        [LoggerMessage(101, LogLevel.Debug, "The Docker container with identifier {containerId} does not contain the '{containerLabel}' expected label")]
        public static partial void DockerContainerDoesNotContainExpectedLabel(this ILogger serviceLogger, string containerId, string containerLabel);

        [LoggerMessage(102, LogLevel.Debug, "The Docker container with identifier {containerId} does not contain the '{containerLabel}' expected label with the value '{value}'")]
        public static partial void DockerContainerDoesNotContainExpectedLabel(this ILogger serviceLogger, string containerId, string containerLabel, string value);

        [LoggerMessage(103, LogLevel.Debug, "The Docker inspector detected Docker container with identifier {containerId}, which can be registered as a service named '{name}'")]
        public static partial void DockerInspectorDetectedDockerContainer(this ILogger serviceLogger, string containerId, string? name);

        [LoggerMessage(104, LogLevel.Debug, "Detected and ignored new not supported Docker {eventType} event ({eventAction}) about Docker container with identifier {containerId}")]
        public static partial void DockerReturnedNotSupportedEvent(this ILogger serviceLogger, string eventType, string eventAction, string containerId);

        [LoggerMessage(105, LogLevel.Debug, "The Docker inspector defined the service name '{name}' for Docker container with identifier {containerId}")]
        public static partial void DockerInspectorDefinedServiceName(this ILogger serviceLogger, string containerId, string name);

        [LoggerMessage(106, LogLevel.Debug, "Detected changes in network configurations in the Docker container with identifier {containerId}")]
        public static partial void DockerContainerNetworkConfigurationChanged(this ILogger serviceLogger, string containerId);

        [LoggerMessage(107, LogLevel.Debug, "Detected task ARN '{taskArn}' in Docker container with identifier {containerId}")]
        public static partial void DockerInspectorDetectedTaskArn(this ILogger serviceLogger, string containerId, string taskArn);

        [LoggerMessage(300, LogLevel.Warning, "The Docker container with identifier {containerId} cannot be inspected because it is disposed and there is no information about it in the cache")]
        public static partial void CannotInspectDisposedDockerContainer(this ILogger serviceLogger, string containerId);

        [LoggerMessage(301, LogLevel.Warning, "The Docker container with identifier {containerId} cannot be inspected because the container does not exist")]
        public static partial void CannotInspectNotExistedDockerContainer(this ILogger serviceLogger, string containerId);

        [LoggerMessage(400, LogLevel.Error, $"The same task ARN '{{taskArn}}' was detected for multiple Docker containers. The container label {DockerInspector.TaskArnLabel} may have been added manually")]
        public static partial void DockerInspectorDetectedDuplicateTaskArn(this ILogger serviceLogger, string taskArn);

        [LoggerMessage(401, LogLevel.Error, "Failed to parse task ARN for Docker container with identifier {containerId}")]
        public static partial void CannotParseTaskArn(this ILogger serviceLogger, string containerId);

        public static void ConsulRequestMessageCreated(this ILogger serviceLogger, HttpRequestMessage message, string? content = default)
        {
            serviceLogger.ConsulRequestMessageCreated(message.Method.Method, message.RequestUri, content);
        }

        public static void ConsulRequestMessageContainsToken(this ILogger serviceLogger, HttpRequestMessage message)
        {
            serviceLogger.ConsulRequestMessageContainsToken(message.Method.Method, message.RequestUri);
        }

        public static void DockerContainerContainsLabels(this ILogger serviceLogger, string containerId, IDictionary<string, string> containerLabels)
        {
            if (serviceLogger.IsEnabled(LogLevel.Trace))
            {
                serviceLogger.DockerContainerContainsLabels(containerId,
                    string.Join(", ", containerLabels.Select(containerLabel => $"{containerLabel.Key}={containerLabel.Value}")));
            }
        }

        public static void DockerInspectorDetectedDockerContainer(this ILogger serviceLogger, DockerContainer container, string? serviceName)
        {
            if (serviceLogger.IsEnabled(LogLevel.Trace))
            {
                serviceLogger.DockerInspectorDetectedDockerContainer(
                    serviceName, container.Id, SerializeContainerNetworks(container));
            }
        }

        public static void DockerContainerNetworkConfigurationChanged(this ILogger serviceLogger, string containerId, DockerContainer container, DockerContainer cachedContainer)
        {
            serviceLogger.DockerContainerNetworkConfigurationChanged(
                containerId, SerializeContainerNetworks(container), SerializeContainerNetworks(cachedContainer));
        }

        public static void DockerInspectorDefinedServiceName(this ILogger serviceLogger, string containerId, AmazonTask describedTask)
        {
            serviceLogger.DockerInspectorDefinedServiceName(
                containerId, describedTask.Cluster, describedTask.Group, describedTask.Arn);
        }

        private static string? SerializeContainerNetworks(DockerContainer container)
        {
            return string.Join(", ", container.Networks.Select(network =>
            {
                return network.Value == default ? network.Key : $"{network.Key} ({network.Value})";
            }));
        }
    }
}
