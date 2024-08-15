using Consul.Extensions.ContainerInspector.Configurations.Models;
using Consul.Extensions.ContainerInspector.Core.Models;
using Consul.Extensions.ContainerInspector.Extensions;
using Microsoft.Extensions.Logging;

namespace Consul.Extensions.ContainerInspector.Core.Internal
{
    /// <summary>
    /// Default implementation of <see cref="IDockerInspector" />.
    /// </summary>
    internal class DockerInspector(
        DockerInspectorConfiguration configuration,
        IDockerClient docker,
        IAmazonClient aws,
        ILogger<IDockerInspector>? inspectorLogger) : IDockerInspector
    {
        private static readonly IDictionary<string, DockerInspectorEventType> _inspectorEventTypeMap
            = new Dictionary<string, DockerInspectorEventType>
            {
                { "start", DockerInspectorEventType.ContainerDetected },
                { "pause", DockerInspectorEventType.ContainerPaused },
                { "unpause", DockerInspectorEventType.ContainerUnpaused },
                { "die", DockerInspectorEventType.ContainerDisposed },
                { "healthy", DockerInspectorEventType.ContainerHealthy },
                { "unhealthy", DockerInspectorEventType.ContainerUnhealthy },
            };

        private static readonly HashSet<string> _supportedActions
            = new(_inspectorEventTypeMap.Keys.Union(["connect", "disconnect"]));

        public IAsyncEnumerable<DockerInspectorEvent> InspectAsync(CancellationToken cancellationToken)
        {
            return new Inspector(configuration, docker, aws, inspectorLogger, cancellationToken).InspectAsync();
        }

        private static DockerInspectorEvent CreateInspectorDisposingEvent(string containerId)
        {
            return new DockerInspectorEvent(DockerInspectorEventType.ContainerDisposed)
            {
                Descriptor = new DockerInspectorEventDescriptor(containerId)
            };
        }

        /// <summary>
        /// Returns true if the compared container networks are equal, false otherwise.
        /// </summary>
        private static bool CompareContainerNetworks(ContainerDescriptor containerDescriptor, DockerContainer container)
        {
            return !containerDescriptor.Container.Networks.Except(container.Networks).Concat(
                container.Networks.Except(containerDescriptor.Container.Networks)).Any();
        }

        private class Inspector
        {
            private readonly DockerInspectorConfiguration _configuration;
            private readonly IDockerClient _docker;
            private readonly IAmazonClient _aws;
            private readonly ILogger<IDockerInspector>? _inspectorLogger;
            private readonly CancellationToken _cancellationToken;
            private readonly Dictionary<string, ContainerDescriptor> _containerDescriptors = [];

            public Inspector(
                DockerInspectorConfiguration configuration,
                IDockerClient docker,
                IAmazonClient aws,
                ILogger<IDockerInspector>? inspectorLogger,
                CancellationToken cancellationToken)
            {
                _configuration = configuration;
                _docker = docker;
                _aws = aws;
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
                    _inspectorLogger?.DockerInspectorDetectedDockerContainer(
                        containerDescriptor.Container, containerDescriptor.ServiceName);

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

                DockerInspectorEvent? inspectorEvent;
                await foreach (var containerEvent in _docker.MonitorAsync(currentDate, _cancellationToken))
                {
                    if (!_supportedActions.Contains(containerEvent.EventAction))
                    {
                        _inspectorLogger?.DockerReturnedNotSupportedEvent(
                            containerEvent.EventType, containerEvent.EventAction);

                        continue;
                    }

                    if ((inspectorEvent = await InspectAsync(containerEvent)) != default)
                    {
                        yield return inspectorEvent;
                    }
                }
            }

            private async Task<DockerInspectorEvent?> InspectAsync(DockerContainerEvent containerEvent)
            {
                // If possible, it is necessary to take information about the container from the
                // local cache. But if there is no information about the container in the cache yet,
                // then try to get it from Docker (if this is not a container deletion event).
                ContainerDescriptor? descriptor = default;
                if (!_containerDescriptors.TryGetValue(containerEvent.ContainerId, out var cachedDescriptor))
                {
                    using (_inspectorLogger?.CreateContainerScope(containerEvent.ContainerId))
                    {
                        _inspectorLogger?.DockerContainerDescriptorNotFoundInCache();

                        if (containerEvent.EventAction == "die")
                        {
                            _inspectorLogger?.CannotInspectDisposedDockerContainer();
                            return CreateInspectorDisposingEvent(containerEvent.ContainerId);
                        }
                    }

                    var container = await _docker.GetContainerAsync(containerEvent.ContainerId, _cancellationToken);
                    if (container == default)
                    {
                        using (_inspectorLogger?.CreateContainerScope(containerEvent.ContainerId))
                        {
                            _inspectorLogger?.CannotInspectNotExistedDockerContainer();
                        }

                        // This code can only be reachable if Docker events are processed more
                        // slowly than new events are coming in. For example, if a Docker container
                        // starts and immediately dies. It is possible to ignore this and wait for
                        // the "die" event.
                        return default;
                    }

                    descriptor = await InspectContainerAsync(container);
                    if ((_containerDescriptors[container.Id] = descriptor).ServiceName?.Length > 0)
                    {
                        using (_inspectorLogger?.CreateContainerScope(container.Id))
                        {
                            _inspectorLogger?.DockerInspectorDefinedServiceName(descriptor.ServiceName!);
                        }
                    }
                }
                else if (containerEvent.EventAction == "die")
                {
                    _containerDescriptors.Remove(containerEvent.ContainerId);
                }

                if (containerEvent.EventType == "network")
                {
                    // Compare Docker container IP addresses only if the Docker container information
                    // is loaded from the cache and the service name is set. Otherwise, we will not
                    // be able to compare whether the Docker container IP addresses have changed.
                    if (cachedDescriptor?.ServiceName?.Length > 0)
                    {
                        var container = await _docker.GetContainerAsync(containerEvent.ContainerId, _cancellationToken);
                        if (container == default || CompareContainerNetworks(cachedDescriptor, container))
                        {
                            return default;
                        }

                        _containerDescriptors[container.Id] = await InspectContainerAsync(container);

                        using (_inspectorLogger?.CreateContainerScope(container.Id))
                        {
                            _inspectorLogger?.DockerContainerNetworkConfigurationChanged(
                                container, cachedDescriptor.Container);
                        }

                        return _containerDescriptors[container.Id].CreateInspectorEvent(
                            DockerInspectorEventType.ContainerNetworksUpdated);
                    }
                }
                else if ((descriptor ??= cachedDescriptor)?.ServiceName?.Length > 0)
                {
                    var inspectorEventType = _inspectorEventTypeMap[containerEvent.EventAction];

                    if (cachedDescriptor != default)
                    {
                        switch (inspectorEventType)
                        {
                            case DockerInspectorEventType.ContainerHealthy:
                                cachedDescriptor.Container.IsHealthy = true;
                                break;

                            case DockerInspectorEventType.ContainerPaused:
                                cachedDescriptor.Container.IsSuspended = true;
                                break;

                            case DockerInspectorEventType.ContainerUnhealthy:
                                cachedDescriptor.Container.IsHealthy = default;
                                break;

                            case DockerInspectorEventType.ContainerUnpaused:
                                cachedDescriptor.Container.IsSuspended = default;
                                break;
                        }
                    }

                    return descriptor!.CreateInspectorEvent(inspectorEventType);
                }

                return default;
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

                    using (_inspectorLogger?.CreateContainerScope(container.Id))
                    {
                        _inspectorLogger?.DockerInspectorDefinedServiceName(serviceName, _configuration.Labels.ServiceLabel);
                    }

                    yield return new ContainerDescriptor(container, serviceName);
                }

                var arns = new Dictionary<AmazonTaskArn, ContainerDescriptor>();
                while (containersQueue.TryDequeue(out var container))
                {
                    using (_inspectorLogger?.CreateContainerScope(container.Id))
                    {
                        try
                        {
                            var parsedArn = AmazonTaskArn.GetTaskArn(container);
                            if (parsedArn == default)
                            {
                                continue;
                            }

                            if (arns.Keys.Any(arn => arn == parsedArn))
                            {
                                _inspectorLogger?.DockerInspectorDetectedDuplicateTaskArn(parsedArn.EncodedArn);
                                continue;
                            }

                            _inspectorLogger?.DockerInspectorDetectedTaskArn(parsedArn.EncodedArn);
                            arns.Add(parsedArn, new ContainerDescriptor(container, default));
                        }
                        catch (TaskArnParseException ex)
                        {
                            _inspectorLogger?.CannotParseTaskArn(ex.EncodedArn);
                        }
                    }
                }

                foreach (var describedTask in await _aws.DescribeTasksAsync(arns.Keys, _cancellationToken))
                {
                    if (arns.TryGetValue(describedTask.TaskArn, out var containerDescriptor))
                    {
                        using (_inspectorLogger?.CreateContainerScope(containerDescriptor.Container.Id))
                        {
                            _inspectorLogger?.DockerInspectorDefinedServiceName(describedTask);
                        }

                        yield return new ContainerDescriptor(containerDescriptor.Container, describedTask.Group);
                    }
                    else
                    {
                        // The AWS client never returns tasks with an ARN that not requested.
                        // Therefore this code should not be reached.
                        throw new InvalidOperationException(
                            "The AWS client returned ECS task with an ARN that was not requested.");
                    }
                }
            }

            private async Task<ContainerDescriptor> InspectContainerAsync(DockerContainer container)
            {
                await foreach (var descriptor in InspectContainersAsync([container]))
                {
                    return descriptor;
                }

                return new ContainerDescriptor(container, default);
            }
        }

        private record ContainerDescriptor(DockerContainer Container, string? ServiceName)
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
    }
}
