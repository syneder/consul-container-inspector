using Consul.Extensions.ContainerInspector.Core.Configuration.Models;
using Consul.Extensions.ContainerInspector.Core.Models;
using Consul.Extensions.ContainerInspector.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Consul.Extensions.ContainerInspector.Core.Internal
{
    internal class DockerInspector(
        IDocker docker,
        IOptions<DockerInspectorConfiguration> options,
        ILogger<DockerInspector>? inspectorLogger) : IDockerInspector
    {
        private static readonly IDictionary<string, DockerInspectorEventType> _inspectorEventTypeMap
            = new Dictionary<string, DockerInspectorEventType>
            {
                { "start", DockerInspectorEventType.ContainerDetected },
                { "pause", DockerInspectorEventType.ContainerPaused },
                { "unpause", DockerInspectorEventType.ContainerUnpaused },
                { "die", DockerInspectorEventType.ContainerDisposed }
            };

        private static readonly HashSet<string> _supportedActions
            = new(_inspectorEventTypeMap.Keys.Union(["connect", "disconnect"]));

        public const string ResourceArnLabel = "com.amazonaws.ecs.task-arn";

        public IAsyncEnumerable<DockerInspectorEvent> InspectAsync(CancellationToken cancellationToken)
        {
            var configuration = options?.Value
                ?? throw new InvalidOperationException("IOptions<DockerInspectorConfiguration> is not configured.");

            return new Inspector(docker, inspectorLogger, configuration, cancellationToken).InspectAsync();
        }

        private static DockerInspectorEvent CreateInspectorDisposingEvent(string containerId)
        {
            return new DockerInspectorEvent(DockerInspectorEventType.ContainerDisposed)
            {
                Descriptor = new DockerInspectorEventDescriptor(containerId)
            };
        }

        private class Inspector
        {
            private readonly IDocker _docker;
            private readonly ILogger<DockerInspector>? _inspectorLogger;
            private readonly DockerInspectorConfiguration _configuration;
            private readonly CancellationToken _cancellationToken;

            private readonly Dictionary<string, ContainerDescriptor> _containerDescriptors = [];

            public Inspector(
                IDocker docker,
                ILogger<DockerInspector>? inspectorLogger,
                DockerInspectorConfiguration configuration,
                CancellationToken cancellationToken)
            {
                _docker = docker;
                _inspectorLogger = inspectorLogger;
                _cancellationToken = cancellationToken;
            }

            public async IAsyncEnumerable<DockerInspectorEvent> InspectAsync()
            {
                // Since running containers will take some time to process, new events associated
                // with those containers may be lost during this time. Therefore, we will commit
                // the current time and use it when tracking events to handle any lost events.
                var currentDate = DateTime.Now;

                var containers = await _docker.GetContainersAsync(_cancellationToken);
                await foreach (var containerDescriptor in InspectContainersAsync(containers.ToArray()))
                {
                    if (containerDescriptor.ServiceName?.Length > 0)
                    {
                        yield return containerDescriptor.CreateInspectorEvent(
                            DockerInspectorEventType.ContainerDetected);
                    }

                    _containerDescriptors.Add(containerDescriptor.Container.Id, containerDescriptor);
                }

                // Dispatch of the ContainersInspectionCompleted event indicates that all running
                // containers have completed processing, which means that there are no more running
                // containers at this time.
                yield return new DockerInspectorEvent(DockerInspectorEventType.ContainersInspectionCompleted);

                await foreach (var containerEvent in _docker.MonitorAsync(currentDate, _cancellationToken))
                {
                    if (!_supportedActions.Contains(containerEvent.EventAction))
                    {
                        _inspectorLogger?.DockerEventIgnored(
                            containerEvent.EventType, containerEvent.EventAction, containerEvent.ContainerId);

                        continue;
                    }

                    await foreach (var inspectorEvent in InspectAsync(containerEvent))
                    {
                        yield return inspectorEvent;
                    }
                }
            }

            private async IAsyncEnumerable<DockerInspectorEvent> InspectAsync(DockerContainerEvent containerEvent)
            {
                // If possible, it is necessary to take information about the container from the
                // local cache. But if there is no information about the container in the cache yet,
                // then try to get it from Docker (if this is not a container deletion event).
                ContainerDescriptor? descriptor = default;
                if (!_containerDescriptors.TryGetValue(containerEvent.ContainerId, out var cachedDescriptor))
                {
                    if (containerEvent.EventAction == "die")
                    {
                        _inspectorLogger?.CannotInspectDisposedDockerContainer(containerEvent.ContainerId);

                        yield return CreateInspectorDisposingEvent(containerEvent.ContainerId);
                        yield break;
                    }

                    var container = await _docker.GetContainerAsync(containerEvent.ContainerId, _cancellationToken);
                    if (container == default)
                    {
                        _inspectorLogger?.CannotInspectNotExistedDockerContainer(containerEvent.ContainerId);

                        yield return CreateInspectorDisposingEvent(containerEvent.ContainerId);
                        yield break;
                    }

                    descriptor = await InspectContainerAsync(container);
                    if ((_containerDescriptors[container.Id] = descriptor).ServiceName?.Length > 0)
                    {
                        yield return descriptor.CreateInspectorEvent(DockerInspectorEventType.ContainerDetected);
                    }
                }

                if (containerEvent.EventType == "network")
                {
                    // Compare Docker container IP addresses only if the Docker container information
                    // is loaded from the cache and the service name is set. Otherwise, we will not
                    // be able to compare whether the Docker container IP addresses have changed.
                    if (cachedDescriptor?.ServiceName?.Length > 0)
                    {
                        var container = await _docker.GetContainerAsync(containerEvent.ContainerId, _cancellationToken);
                        if (container == default)
                        {
                            _inspectorLogger?.CannotInspectNotExistedDockerContainer(containerEvent.ContainerId);
                            _containerDescriptors.Remove(cachedDescriptor.Container.Id);

                            yield return CreateInspectorDisposingEvent(containerEvent.ContainerId);
                            yield break;
                        }

                        _containerDescriptors[container.Id] = await InspectContainerAsync(container);
                        if (cachedDescriptor.Container.Networks.Except(container.Networks).Any())
                        {
                            yield return _containerDescriptors[container.Id].CreateInspectorEvent(
                                DockerInspectorEventType.ContainerNetworksUpdated);
                        }
                    }

                    yield break;
                }

                Debug.Assert(!(descriptor == default && cachedDescriptor == default));

                yield return (descriptor ?? cachedDescriptor)!.CreateInspectorEvent(
                    _inspectorEventTypeMap[containerEvent.EventAction]);
            }

            private async IAsyncEnumerable<ContainerDescriptor> InspectContainersAsync(DockerContainer[] containers)
            {
                // Let's remember containers that do not contain a label with the name of the service.
                // These containers can contain another com.amazonaws.ecs.task-arn label, indicating
                // that the container is managed by Amazon ECS Anywhere. The service name for such
                // containers can be taken from the service name in the ECS cluster.
                var containersQueue = new Queue<DockerContainer>();
                foreach (var container in containers)
                {
                    if (!container.Labels.TryGetValue(_configuration.Labels.ServiceLabel, out var serviceName))
                    {
                        containersQueue.Enqueue(container);
                        continue;
                    }

                    yield return new ContainerDescriptor(container, serviceName, default);
                }

                var resourceArns = new Dictionary<string, ContainerDescriptor>();
                while (containersQueue.TryDequeue(out var container))
                {
                    try
                    {
                        var resourceArn = ResourceArn.GetResourceArn(container);
                        if (resourceArn == default)
                        {
                            continue;
                        }

                        if (resourceArns.ContainsKey(resourceArn.Arn))
                        {
                            _inspectorLogger?.DuplicateResourceArn(resourceArn.Arn);
                            continue;
                        }

                        resourceArns.Add(resourceArn.Arn, new ContainerDescriptor(container, default, resourceArn));
                    }
                    catch (ResourceArnException)
                    {
                        _inspectorLogger?.ResourceArnInvalid(container.Id);
                    }
                }

                // To reduce the number of API requests to AWS, group the found resource ARNs by
                // region and then by cluster. In a standard configuration, only one group will exist,
                // since the same container instance can only be connected to one cluster. But this
                // rule can be broken by using a certain configuration.
                foreach (var regionGroup in resourceArns.Values.GroupBy(descriptor => descriptor.ResourceArn!.Region))
                {
                    foreach (var clusterGroup in regionGroup.GroupBy(descriptor => descriptor.ResourceArn!.Cluster))
                    {
                        // TODO: Add request to AWS
                        // Cluster = clusterGroup.Key,
                        // Tasks = clusterGroup.Select(descriptor => descriptor.ResourceArn!.Arn)
                    }
                }
            }

            private async Task<ContainerDescriptor> InspectContainerAsync(DockerContainer container)
            {
                await foreach (var descriptor in InspectContainersAsync([container]))
                {
                    return descriptor;
                }

                return new ContainerDescriptor(container, default, default);
            }
        }

        private record ContainerDescriptor(DockerContainer Container, string? ServiceName, ResourceArn? ResourceArn)
        {
            public DockerInspectorEvent CreateInspectorEvent(DockerInspectorEventType eventType)
            {
                return new DockerInspectorEvent(eventType)
                {
                    ServiceName = ServiceName,
                    Descriptor = new DockerInspectorEventDescriptor(Container),
                };
            }
        }

        private record ResourceArn(string Arn, string ResourceId, string Region, string Cluster)
        {
            public static ResourceArn? GetResourceArn(DockerContainer container)
            {
                if (!container.Labels.TryGetValue(ResourceArnLabel, out var resourceArn))
                {
                    return default;
                }

                // In the current implementation, ARN creation is only allowed for ECS tasks. The ARN
                // has the following format, but not all components of the ARN are required and will be
                // available once created: arn:{partition}:{service}:{region}:{account-id}:ecs/{cluster}/{resourceId}
                if (resourceArn.Split(':') is [_, _, _, var region, _, var resource] &&
                    resource.Split('/') is [_, var cluster, var resourceId])
                {
                    return new ResourceArn(resourceArn, resourceId, region, cluster);
                }

                throw new ResourceArnException();
            }
        }

        private class ResourceArnException : Exception { }
    }
}
