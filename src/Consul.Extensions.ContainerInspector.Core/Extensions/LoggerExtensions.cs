using Consul.Extensions.ContainerInspector.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;

namespace Consul.Extensions.ContainerInspector.Extensions
{
    public static partial class LoggerExtensions
    {
        private static readonly Func<ILogger, HttpMethod, Uri?, IDisposable?> _requestScopeFactory = LoggerMessage.DefineScope<HttpMethod, Uri?>("{requestMethod} {requestUri}");

        [LoggerMessage(1, LogLevel.Trace, "The Docker container with identifier {containerId} contains labels: {serializedLabels}")]
        private static partial void DockerContainerContainsLabels(this ILogger serviceLogger, string containerId, string serializedLabels);

        [LoggerMessage(2, LogLevel.Trace, "The Docker inspector detected Docker container with identifier {containerId}. Possible service name: '{serviceName}'. Container networks: [{containerNetworks}]")]
        private static partial void DockerInspectorDetectedDockerContainer(this ILogger serviceLogger, string? serviceName, string containerId, string? containerNetworks);

        [LoggerMessage(3, LogLevel.Trace, "Detected changes in network configurations in the Docker container with identifier {containerId}. Current networks: [{currentNetworks}]. Previous networks: [{networks}])")]
        private static partial void DockerContainerNetworkConfigurationChanged(this ILogger serviceLogger, string containerId, string? currentNetworks, string? networks);

        [LoggerMessage(4, LogLevel.Trace, "The Docker inspector defined the service name '{serviceName}' for the Docker container with identifier {containerId} using the name of the running task '{taskArn}' in the AWS ECS cluster named '{clusterName}'")]
        private static partial void DockerInspectorDefinedServiceName(this ILogger serviceLogger, string containerId, string clusterName, string serviceName, string taskArn);

        [LoggerMessage(100, LogLevel.Debug, "Start processing HTTP request")]
        private static partial void RequestScopeCreated(this ILogger serviceLogger);

        [LoggerMessage(101, LogLevel.Debug, "End processing HTTP request after {elapsedMilliseconds}ms with status code {statusCode}")]
        private static partial void RequestScopeCompleted(this ILogger serviceLogger, long elapsedMilliseconds, HttpStatusCode statusCode);

        [LoggerMessage(102, LogLevel.Debug, "Sending HTTP request")]
        private static partial void RequestCreated(this ILogger serviceLogger);

        [LoggerMessage(103, LogLevel.Debug, "Received HTTP response headers after {elapsedMilliseconds}ms with status code {statusCode}")]
        private static partial void RequestCompleted(this ILogger serviceLogger, long elapsedMilliseconds, HttpStatusCode statusCode);

        [LoggerMessage(104, LogLevel.Debug, "Detected changes in network configurations in the Docker container with identifier {containerId}")]
        private static partial void DockerContainerNetworkConfigurationChanged(this ILogger serviceLogger, string containerId);

        [LoggerMessage(200, LogLevel.Information, "The Consul service '{serviceName}' with identifier '{serviceId}' unregistered")]
        private static partial void ServiceUnregistered(this ILogger serviceLogger, string serviceId, string serviceName);

        [LoggerMessage(201, LogLevel.Information, "The Consul service '{serviceName}' with identifier '{serviceId}' unregistered because the corresponding Docker container with identifier {containerId} has been suspended, stopped, unhealthy or died")]
        private static partial void ServiceUnregistered(this ILogger serviceLogger, string containerId, string serviceId, string serviceName);

        [LoggerMessage(202, LogLevel.Information, "The Consul service '{serviceName}' with identifier '{serviceId}' has been registered or updated")]
        private static partial void ServiceRegistered(this ILogger serviceLogger, string serviceId, string serviceName);

        [LoggerMessage(203, LogLevel.Information, "The Consul service '{serviceName}' with identifier '{serviceId}' has been registered or updated with IP address {address}")]
        private static partial void ServiceRegistered(this ILogger serviceLogger, string serviceId, string serviceName, string address);

        [LoggerMessage(20, LogLevel.Trace, "The Docker client received a message about a new Docker event. Event content: {eventContent}")]
        internal static partial void DockerSentEventMessage(this ILogger serviceLogger, string eventContent);

        [LoggerMessage(21, LogLevel.Trace, "Descriptor for the Docker container with identifier {containerId} was not found in the cache")]
        internal static partial void DockerContainerDescriptorNotFoundInCache(this ILogger serviceLogger, string containerId);

        [LoggerMessage(22, LogLevel.Trace, "The Docker inspector defined the service name '{serviceName}' using the Docker container label named '{containerLabel}' for Docker container with identifier {containerId}")]
        internal static partial void DockerInspectorDefinedServiceName(this ILogger serviceLogger, string containerId, string containerLabel, string serviceName);

        [LoggerMessage(23, LogLevel.Trace, "Detected task ARN '{taskArn}' in Docker container with identifier {containerId}")]
        internal static partial void DockerInspectorDetectedTaskArn(this ILogger serviceLogger, string containerId, string taskArn);

        [LoggerMessage(120, LogLevel.Debug, "The Docker container with identifier {containerId} does not exist or was deleted before the API request completed")]
        internal static partial void DockerContainerNotFound(this ILogger serviceLogger, string containerId);

        [LoggerMessage(121, LogLevel.Debug, "Detected and ignored new not supported Docker {eventType} event ({eventAction})")]
        internal static partial void DockerReturnedNotSupportedEvent(this ILogger serviceLogger, string eventType, string eventAction);

        [LoggerMessage(122, LogLevel.Debug, "The Docker container with identifier {containerId} does not contain the '{containerLabel}' expected label")]
        internal static partial void DockerContainerDoesNotContainExpectedLabel(this ILogger serviceLogger, string containerId, string containerLabel);

        [LoggerMessage(123, LogLevel.Debug, "The Docker container with identifier {containerId} does not contain the '{containerLabel}' label with the expected value '{expectedValue}'")]
        internal static partial void DockerContainerDoesNotContainExpectedLabel(this ILogger serviceLogger, string containerId, string containerLabel, string expectedValue);

        [LoggerMessage(124, LogLevel.Debug, "The Docker inspector defined the service name '{serviceName}' for Docker container with identifier {containerId}")]
        internal static partial void DockerInspectorDefinedServiceName(this ILogger serviceLogger, string containerId, string serviceName);

        [LoggerMessage(320, LogLevel.Warning, "The Docker container with identifier {containerId} cannot be inspected because it is disposed and there is no information about it in the cache")]
        internal static partial void CannotInspectDisposedDockerContainer(this ILogger serviceLogger, string containerId);

        [LoggerMessage(321, LogLevel.Warning, "The Docker container with identifier {containerId} cannot be inspected because the container does not exist")]
        internal static partial void CannotInspectNotExistedDockerContainer(this ILogger serviceLogger, string containerId);

        [LoggerMessage(420, LogLevel.Error, $"The same task ARN '{{taskArn}}' was detected for multiple Docker containers. The container label {AmazonTaskArn.ContainerLabel} may have been added manually")]
        internal static partial void DockerInspectorDetectedDuplicateTaskArn(this ILogger serviceLogger, string taskArn);

        [LoggerMessage(421, LogLevel.Error, "Failed to parse task ARN '{taskArn}' for Docker container with identifier {containerId}")]
        internal static partial void CannotParseTaskArn(this ILogger serviceLogger, string containerId, string taskArn);

        [LoggerMessage(140, LogLevel.Debug, "The registered Consul service '{serviceName}' (id: {serviceId}) does not have a corresponding Docker container identifier, so this service will be ignored")]
        public static partial void ServiceDoesNotContainContainerId(this ILogger serviceLogger, string serviceId, string serviceName);

        [LoggerMessage(141, LogLevel.Debug, "It is not possible to use the same service identifier '{registeredServiceId}' because the service name '{serviceName}' is different from the registered service name '{registeredServiceName}'")]
        public static partial void CannotUseRegisteredServiceId(this ILogger serviceLogger, string registeredServiceId, string registeredServiceName, string serviceName);

        [LoggerMessage(142, LogLevel.Debug, "The IP address of the service named '{serviceName}' cannot be determined for container identifier {containerId}")]
        public static partial void CannotDetermineServiceIPAddress(this ILogger serviceLogger, string containerId, string serviceName);

        [LoggerMessage(143, LogLevel.Debug, "The IP address of the service named '{serviceName}' cannot be determined for container identifier {containerId} because this container has multiple IP addresses assigned to it")]
        public static partial void CannotUseMultipleServiceIPAddresses(this ILogger serviceLogger, string containerId, string serviceName);

        [LoggerMessage(440, LogLevel.Error, "The same Docker container identifier {containerId} is specified for multiple registered Consul services, so the registered service '{serviceName}' with identifier '{serviceId}' will be unregistered")]
        public static partial void ServiceContainsDuplicateContainerId(this ILogger serviceLogger, string containerId, string serviceId, string serviceName);

        internal static IDisposable? CreateRequestScope(this ILogger serviceLogger, HttpRequestMessage request)
        {
            return _requestScopeFactory(serviceLogger, request.Method, request.RequestUri);
        }

        internal static void RequestScopeCreated(this ILogger serviceLogger, HttpRequestMessage request)
        {
            serviceLogger.RequestScopeCreated();
        }

        internal static void RequestScopeCompleted(this ILogger serviceLogger, HttpResponseMessage response, Stopwatch stopwatch)
        {
            serviceLogger.RequestScopeCompleted(stopwatch.ElapsedMilliseconds, response.StatusCode);
        }

        internal static void RequestCreated(this ILogger serviceLogger, HttpRequestMessage request)
        {
            serviceLogger.RequestCreated();
        }

        internal static void RequestCompleted(this ILogger serviceLogger, HttpResponseMessage response, Stopwatch stopwatch)
        {
            serviceLogger.RequestCompleted(stopwatch.ElapsedMilliseconds, response.StatusCode);
        }

        internal static void DockerContainerContainsLabels(this ILogger serviceLogger, string containerId, IDictionary<string, string> containerLabels)
        {
            if (serviceLogger.IsEnabled(LogLevel.Trace))
            {
                serviceLogger.DockerContainerContainsLabels(containerId,
                    string.Join(", ", containerLabels.Select(containerLabel => $"{containerLabel.Key}={containerLabel.Value}")));
            }
        }

        internal static void DockerInspectorDetectedDockerContainer(this ILogger serviceLogger, DockerContainer container, string? serviceName)
        {
            if (serviceLogger.IsEnabled(LogLevel.Trace))
            {
                serviceLogger.DockerInspectorDetectedDockerContainer(
                    serviceName, container.Id, SerializeContainerNetworks(container));
            }
        }

        internal static void DockerContainerNetworkConfigurationChanged(this ILogger serviceLogger, string containerId, DockerContainer container, DockerContainer cachedContainer)
        {
            if (serviceLogger.IsEnabled(LogLevel.Trace))
            {
                serviceLogger.DockerContainerNetworkConfigurationChanged(
                    containerId, SerializeContainerNetworks(container), SerializeContainerNetworks(cachedContainer));
            }
            else
            {
                serviceLogger.DockerContainerNetworkConfigurationChanged(container.Id);
            }
        }

        internal static void DockerInspectorDefinedServiceName(this ILogger serviceLogger, string containerId, AmazonTask describedTask)
        {
            serviceLogger.DockerInspectorDefinedServiceName(
                containerId, describedTask.Arn.Cluster, describedTask.Group, describedTask.Arn.Arn);
        }

        public static void ServiceRegistered(this ILogger serviceLogger, ServiceRegistration service)
        {
            if (service.Address == default)
            {
                serviceLogger.ServiceRegistered(service.Id, service.Name);
                return;
            }

            serviceLogger.ServiceRegistered(service.Id, service.Name, service.Address);
        }

        public static void ServiceUnregistered(this ILogger serviceLogger, ServiceRegistration service, string? containerId = default)
        {
            if (containerId == default)
            {
                serviceLogger.ServiceUnregistered(service.Id, service.Name);
                return;
            }

            serviceLogger.ServiceUnregistered(service.Id, service.Name, containerId);
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
