using Consul.Extensions.ContainerInspector.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;

namespace Consul.Extensions.ContainerInspector.Extensions
{
    public static partial class LoggerExtensions
    {
        private static readonly Func<ILogger, HttpMethod, Uri?, IDisposable?> _requestScopeFactory = LoggerMessage.DefineScope<HttpMethod, Uri?>("[ {requestMethod} {requestUri} ]");
        private static readonly Func<ILogger, string, IDisposable?> _containerScopeFactory = LoggerMessage.DefineScope<string>("[ Docker container {containerId} ]");
        private static readonly Func<ILogger, string, IDisposable?> _serviceScopeFactory = LoggerMessage.DefineScope<string>("[ Consul service '{serviceId}' ]");

        [LoggerMessage(10, LogLevel.Trace, "The Docker container contains labels: {serializedLabels}")]
        internal static partial void DockerContainerContainsLabels(this ILogger serviceLogger, string serializedLabels);

        [LoggerMessage(11, LogLevel.Trace, "The Docker client received new event. Event content: {eventContent}")]
        internal static partial void DockerSentEventMessage(this ILogger serviceLogger, string eventContent);

        [LoggerMessage(12, LogLevel.Trace, "Detected Docker container. ID: {containerId}. Service name: '{serviceName}'. Container networks: [{containerNetworks}]")]
        internal static partial void DockerInspectorDetectedDockerContainer(this ILogger serviceLogger, string containerId, string? serviceName, string? containerNetworks);

        [LoggerMessage(13, LogLevel.Trace, "Descriptor for the Docker container was not found in the cache")]
        internal static partial void DockerContainerDescriptorNotFoundInCache(this ILogger serviceLogger);

        [LoggerMessage(14, LogLevel.Trace, "Detected changes in network configurations. Previously connected networks: [{networks}]. Connected networks: [{currentNetworks}]")]
        internal static partial void DockerContainerNetworkConfigurationChanged(this ILogger serviceLogger, string? currentNetworks, string? networks);

        [LoggerMessage(15, LogLevel.Trace, "Defined the service name '{serviceName}' using the Docker container label '{containerLabel}'")]
        internal static partial void DockerInspectorDefinedServiceName(this ILogger serviceLogger, string serviceName, string containerLabel);

        [LoggerMessage(16, LogLevel.Trace, "Defined the service name '{serviceName}' using the ECS service name. ECS cluster: '{clusterName}'. Task ARN: '{taskArn}'")]
        internal static partial void DockerInspectorDefinedServiceName(this ILogger serviceLogger, string serviceName, string clusterName, string taskArn);

        [LoggerMessage(17, LogLevel.Trace, "Detected task ARN '{taskArn}'")]
        internal static partial void DockerInspectorDetectedTaskArn(this ILogger serviceLogger, string taskArn);

        [LoggerMessage(100, LogLevel.Debug, "Start processing HTTP request")]
        internal static partial void RequestScopeCreated(this ILogger serviceLogger);

        [LoggerMessage(101, LogLevel.Debug, "End processing HTTP request after {elapsedMilliseconds}ms with status code {statusCode}")]
        internal static partial void RequestScopeCompleted(this ILogger serviceLogger, long elapsedMilliseconds, HttpStatusCode statusCode);

        [LoggerMessage(102, LogLevel.Debug, "Sending HTTP request")]
        internal static partial void RequestCreated(this ILogger serviceLogger);

        [LoggerMessage(103, LogLevel.Debug, "Received HTTP response headers after {elapsedMilliseconds}ms with status code {statusCode}")]
        internal static partial void RequestCompleted(this ILogger serviceLogger, long elapsedMilliseconds, HttpStatusCode statusCode);

        [LoggerMessage(104, LogLevel.Debug, "The Docker container does not contain the expected label {expectedLabel}")]
        internal static partial void DockerContainerDoesNotContainExpectedLabel(this ILogger serviceLogger, string expectedLabel);

        [LoggerMessage(105, LogLevel.Debug, "The Docker container does not contain the label '{containerLabel}' with the expected value '{expectedValue}'")]
        internal static partial void DockerContainerDoesNotContainExpectedLabel(this ILogger serviceLogger, string containerLabel, string expectedValue);

        [LoggerMessage(106, LogLevel.Debug, "The Docker container does not exist or was deleted before the API request completed")]
        internal static partial void DockerContainerNotFound(this ILogger serviceLogger);

        [LoggerMessage(107, LogLevel.Debug, "Received new not supported Docker {eventType} event. Event action: {eventAction}")]
        internal static partial void DockerReturnedNotSupportedEvent(this ILogger serviceLogger, string eventType, string eventAction);

        [LoggerMessage(108, LogLevel.Debug, "The Docker inspector defined the service name '{serviceName}'")]
        internal static partial void DockerInspectorDefinedServiceName(this ILogger serviceLogger, string serviceName);

        [LoggerMessage(109, LogLevel.Debug, "Detected changes in network configurations")]
        internal static partial void DockerContainerNetworkConfigurationChanged(this ILogger serviceLogger);

        [LoggerMessage(120, LogLevel.Debug, "Consul service does not have a corresponding Docker container ID")]
        public static partial void ServiceDoesNotContainContainerId(this ILogger serviceLogger);

        [LoggerMessage(121, LogLevel.Debug, "The registered service ID cannot be used because the service name has changed")]
        public static partial void RegisteredServiceIdCannotBeUsed(this ILogger serviceLogger);

        [LoggerMessage(122, LogLevel.Debug, "The IP address of the Consul service cannot be determined")]
        public static partial void CannotDetermineServiceIPAddress(this ILogger serviceLogger);

        [LoggerMessage(200, LogLevel.Information, "The Consul service unregistered")]
        public static partial void ServiceUnregistered(this ILogger serviceLogger);

        [LoggerMessage(201, LogLevel.Information, "The Consul service has been registered or updated")]
        public static partial void ServiceRegistered(this ILogger serviceLogger);

        [LoggerMessage(202, LogLevel.Information, "The Consul service has been registered or updated. IP address: {address}")]
        public static partial void ServiceRegistered(this ILogger serviceLogger, string address);

        [LoggerMessage(300, LogLevel.Warning, "The Docker container cannot be inspected because it is disposed")]
        internal static partial void CannotInspectDisposedDockerContainer(this ILogger serviceLogger);

        [LoggerMessage(301, LogLevel.Warning, "The Docker container cannot be inspected because it does not exist")]
        internal static partial void CannotInspectNotExistedDockerContainer(this ILogger serviceLogger);

        [LoggerMessage(302, LogLevel.Warning, "The IP address of the Consul service cannot be determined because Docker container has multiple IP addresses")]
        public static partial void CannotUseMultipleServiceIPAddresses(this ILogger serviceLogger);

        [LoggerMessage(400, LogLevel.Error, "Task ARN '{taskArn}' was detected for multiple Docker containers")]
        internal static partial void DockerInspectorDetectedDuplicateTaskArn(this ILogger serviceLogger, string taskArn);

        [LoggerMessage(401, LogLevel.Error, "Failed to parse task ARN '{taskArn}'")]
        internal static partial void CannotParseTaskArn(this ILogger serviceLogger, string taskArn);

        [LoggerMessage(420, LogLevel.Error, "The same Docker container ID is specified for multiple registered Consul services")]
        public static partial void ServiceContainsDuplicateContainerId(this ILogger serviceLogger);

        internal static IDisposable? CreateRequestScope(this ILogger serviceLogger, HttpRequestMessage request)
        {
            return _requestScopeFactory(serviceLogger, request.Method, request.RequestUri);
        }

        public static IDisposable? CreateContainerScope(this ILogger serviceLogger, string containerId)
        {
            if (containerId.Length > 12)
            {
                containerId = containerId[..12];
            }

            return _containerScopeFactory(serviceLogger, containerId);
        }

        public static IDisposable? CreateServiceScope(this ILogger serviceLogger, string serviceId)
        {
            return _serviceScopeFactory(serviceLogger, serviceId);
        }

        internal static void RequestCreated(this ILogger serviceLogger, HttpRequestMessage request)
        {
            serviceLogger.RequestCreated();
        }

        internal static void RequestCompleted(this ILogger serviceLogger, HttpResponseMessage response, Stopwatch stopwatch)
        {
            serviceLogger.RequestCompleted(stopwatch.ElapsedMilliseconds, response.StatusCode);
        }

        internal static void RequestScopeCreated(this ILogger serviceLogger, HttpRequestMessage request)
        {
            serviceLogger.RequestScopeCreated();
        }

        internal static void RequestScopeCompleted(this ILogger serviceLogger, HttpResponseMessage response, Stopwatch stopwatch)
        {
            serviceLogger.RequestScopeCompleted(stopwatch.ElapsedMilliseconds, response.StatusCode);
        }

        internal static void DockerContainerContainsLabels(this ILogger serviceLogger, IDictionary<string, string> containerLabels)
        {
            if (serviceLogger.IsEnabled(LogLevel.Trace))
            {
                serviceLogger.DockerContainerContainsLabels(string.Join(", ",
                    containerLabels.Select(containerLabel => $"{containerLabel.Key}={containerLabel.Value}")));
            }
        }

        internal static void DockerInspectorDetectedDockerContainer(this ILogger serviceLogger, DockerContainer container, string? serviceName)
        {
            if (serviceLogger.IsEnabled(LogLevel.Trace))
            {
                serviceLogger.DockerInspectorDetectedDockerContainer(
                    container.Id, serviceName, SerializeContainerNetworks(container));
            }
        }

        internal static void DockerContainerNetworkConfigurationChanged(this ILogger serviceLogger, DockerContainer container, DockerContainer cachedContainer)
        {
            if (serviceLogger.IsEnabled(LogLevel.Trace))
            {
                serviceLogger.DockerContainerNetworkConfigurationChanged(
                    SerializeContainerNetworks(container), SerializeContainerNetworks(cachedContainer));
            }
            else
            {
                serviceLogger.DockerContainerNetworkConfigurationChanged();
            }
        }

        internal static void DockerInspectorDefinedServiceName(this ILogger serviceLogger, AmazonTask describedTask)
        {
            serviceLogger.DockerInspectorDefinedServiceName(
                describedTask.Group, describedTask.Arn.Cluster, describedTask.Arn.Arn);
        }

        public static void ServiceRegistered(this ILogger serviceLogger, ServiceRegistration service)
        {
            if (service.Address == default)
            {
                serviceLogger.ServiceRegistered();
                return;
            }

            serviceLogger.ServiceRegistered(service.Address);
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
