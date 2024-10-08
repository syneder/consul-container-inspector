﻿using Consul.Extensions.ContainerInspector.Configurations.Models;
using Consul.Extensions.ContainerInspector.Core;
using Consul.Extensions.ContainerInspector.Core.Models;
using Consul.Extensions.ContainerInspector.Extensions;
using System.Net;

namespace Consul.Extensions.ContainerInspector
{
    public class BackgroundService(
        ManagedInstanceRegistration instanceRegistration,
        ConsulConfiguration configuration,
        IConsulClient consul,
        IDockerInspector dockerInspector,
        ILogger<BackgroundService>? serviceLogger) : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly Dictionary<string, ServiceRegistration> _cache = [];
        private readonly HashSet<string> _containersId = [];

        private CancellationToken _cancellationToken = CancellationToken.None;

        /// <summary>
        /// The name of the Consul service metadata attribute, which contains the identifier of the
        /// corresponding Docker container.
        /// </summary>
        public const string ContainerAttributeName = "container-id";

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            // First of all, get a list of registered services on the Consul agent and the
            // corresponding Docker container identifiers. If the Docker container identifier is
            // missing, the service will be unregistered.
            foreach (var service in await consul.GetServicesAsync(_cancellationToken))
            {
                using (serviceLogger?.CreateServiceScope(service.Id))
                {
                    if (!service.Metadata.TryGetValue(ContainerAttributeName, out var containerId))
                    {
                        serviceLogger?.ServiceDoesNotContainContainerId();
                    }
                    else if (!_cache.TryAdd(containerId, service))
                    {
                        using (serviceLogger?.CreateContainerScope(containerId))
                        {
                            serviceLogger?.ServiceContainsDuplicateContainerId();
                        }

                        await UnregisterServiceAsync(service);
                    }
                }
            }

            try
            {
                await foreach (var inspectorEvent in dockerInspector.InspectAsync(_cancellationToken))
                {
                    if (inspectorEvent.Type == DockerInspectorEventType.ContainersInspectionCompleted)
                    {
                        // The ContainersInspectionCompleted inspector event occurs only once, when the
                        // inspector completes inspecting all running containers. All registered Consul
                        // services with Docker container identifiers for which the inspector did not
                        // return information must be unregistered.
                        foreach (var data in _cache.Where(data => !_containersId.Contains(data.Key)))
                        {
                            using (serviceLogger?.CreateServiceScope(data.Value.Id))
                            {
                                await UnregisterServiceAsync(data.Value);
                            }
                        }

                        continue;
                    }

                    await ProcessDockerInspectorEventAsync(inspectorEvent);
                }
            }
            finally
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    _cancellationToken = CancellationToken.None;
                }

                foreach (var (_, serviceRegistration) in _cache)
                {
                    using (serviceLogger?.CreateServiceScope(serviceRegistration.Id))
                    {
                        await UnregisterServiceAsync(serviceRegistration);
                    }
                }
            }
        }

        private async Task ProcessDockerInspectorEventAsync(DockerInspectorEvent inspectorEvent)
        {
            if (inspectorEvent.Descriptor == default)
            {
                // The descriptor can only be null for an ContainersInspectionCompleted Docker
                // inspector event, which was handled above. This code should never be reached.
                throw new InvalidOperationException("The descriptor in the inspector event is null.");
            }

            _cache.TryGetValue(inspectorEvent.Descriptor.ContainerId, out var cachedService);

            if (default == await ProcessDockerInspectorEventAsync())
            {
                if (cachedService != default)
                {
                    using (serviceLogger?.CreateServiceScope(cachedService.Id))
                    {
                        await UnregisterServiceAsync(cachedService, inspectorEvent.Descriptor.ContainerId);
                    }
                }

                _containersId.Remove(inspectorEvent.Descriptor.ContainerId);
            }

            async Task<ServiceRegistration?> ProcessDockerInspectorEventAsync()
            {
                if (inspectorEvent.Type == DockerInspectorEventType.ContainerDisposed ||
                    inspectorEvent.Type == DockerInspectorEventType.ContainerPaused ||
                    inspectorEvent.Type == DockerInspectorEventType.ContainerUnhealthy)
                {
                    return default;
                }

                var container = inspectorEvent.Descriptor.Container;
                if (container != default && (container.IsSuspended || !container.IsHealthy))
                {
                    return default;
                }

                // If there is no information about service registration yet or the name of the
                // registered service differs from that reported by the Docker inspector, we must create
                // a new service registration. If in this case a registered service exists, it must be
                // deleted before registering a new one.
                var service = cachedService == default || !cachedService.Name.Equals(inspectorEvent.ServiceName)
                    ? CreateServiceRegistration(inspectorEvent)
                    : CreateServiceRegistration(inspectorEvent, cachedService.Id);

                if (service == default)
                {
                    return default;
                }

                if (cachedService != default && !cachedService.Id.Equals(service.Id))
                {
                    using (serviceLogger?.CreateServiceScope(cachedService.Id))
                    {
                        serviceLogger?.RegisteredServiceIdCannotBeUsed();
                        await UnregisterServiceAsync(cachedService, inspectorEvent.Descriptor.ContainerId);
                    }
                }

                using (serviceLogger?.CreateServiceScope(service.Id))
                {
                    await RegisterServiceAsync(service, inspectorEvent.Descriptor.ContainerId);
                }

                _containersId.Add(inspectorEvent.Descriptor.ContainerId);
                return service;
            }
        }

        /// <summary>
        /// Unregisters the specified service from Consul and removes the service registration
        /// from the cache if <paramref name="containerId"/> is specified.
        /// </summary>
        private async Task UnregisterServiceAsync(ServiceRegistration service, string? containerId = default)
        {
            if (containerId == default || _cache.Remove(containerId))
            {
                try
                {
                    await consul.UnregisterServiceAsync(service.Id, _cancellationToken);

                    serviceLogger?.ServiceUnregistered();
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    serviceLogger?.ServiceCannotBeUnregistered();
                }
            }
        }

        /// <summary>
        /// Registers a service in Consul and stores the service registration in cache.
        /// </summary>
        private async Task RegisterServiceAsync(ServiceRegistration service, string containerId)
        {
            await consul.RegisterServiceAsync(_cache[containerId] = service, _cancellationToken);
            serviceLogger?.ServiceRegistered(service);
        }

        /// <summary>
        /// Creates a <see cref="ServiceRegistration" /> using information from the Docker inspector event.
        /// </summary>
        /// <param name="services">List of registered services to create a unique identifier.</param>
        private ServiceRegistration? CreateServiceRegistration(DockerInspectorEvent inspectorEvent)
        {
            if ((inspectorEvent.ServiceName?.Length ?? 0) == 0)
            {
                throw new InvalidOperationException("The service name in inspector event is null or empty.");
            }

            var serviceId = inspectorEvent.ServiceName!.Replace('_', '-');
            if (!ContainsServiceId(serviceId, _cache.Values))
            {
                return CreateServiceRegistration(inspectorEvent, serviceId);
            }

            // Service identifiers must be unique for each Consul agent. By default, the service
            // identifier is equal to its name. If it is necessary to register several different
            // service with the same name on the same Consul agent, this background service adds
            // a unique number to the identifier, starting with _2. To find a unique number, the
            // background service looks for any missing unique number equal to or greater than 2
            // or next after the largest.
            var serviceIndexes = _cache.Values.Select(service =>
            {
                var separatorPosition = service.Id.LastIndexOf('_');
                if (separatorPosition < 0)
                {
                    return default;
                }

                if (service.Id[..separatorPosition].Equals(serviceId, StringComparison.OrdinalIgnoreCase))
                {
                    return int.TryParse(service.Name[(separatorPosition + 1)..], out var index) ? index : default;
                }

                return default;
            });

            var serviceIndex = Enumerable.Range(2, int.MaxValue - 2)
                .Except(serviceIndexes.Where(index => index >= 2).Order())
                .First();

            return CreateServiceRegistration(inspectorEvent, string.Join('_', [serviceId, serviceIndex]));
        }

        /// <summary>
        /// Creates a <see cref="ServiceRegistration" /> with specified <paramref name="serviceId" />
        /// and information from the Docker inspector event.
        /// </summary>
        /// <param name="services">List of registered services to create a unique identifier.</param>
        private ServiceRegistration? CreateServiceRegistration(DockerInspectorEvent inspectorEvent, string serviceId)
        {
            if (inspectorEvent.Descriptor?.Container == default || (inspectorEvent.ServiceName?.Length ?? 0) == 0)
            {
                throw new InvalidOperationException("The service name or descriptor in inspector event is null or empty.");
            }

            using (serviceLogger?.CreateServiceScope(serviceId))
            {
                using (serviceLogger?.CreateContainerScope(inspectorEvent.Descriptor.ContainerId))
                {
                    return CreateServiceRegistration(
                        inspectorEvent.Descriptor.Container, inspectorEvent.ServiceName!, serviceId);
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="ServiceRegistration" /> with specified service <paramref name="name" />,
        /// <paramref name="serviceId" /> and information from the <paramref name="container" />.
        /// </summary>
        private ServiceRegistration? CreateServiceRegistration(DockerContainer container, string name, string serviceId)
        {
            var serviceAddresses = new HashSet<IPAddress?>(container.Networks?.Values.Except([null]) ?? []);
            if (serviceAddresses.Count == 0)
            {
                if (!(container.Networks?.ContainsKey("host") ?? default))
                {
                    serviceLogger?.CannotDetermineServiceIPAddress();
                    return default;
                }

                if (configuration.AdvertiseAddress != default)
                {
                    if (!IPAddress.TryParse(configuration.AdvertiseAddress, out var advertiseAddress))
                    {
                        configuration.AdvertiseAddress = default;
                    }
                    else
                    {
                        serviceAddresses.Add(advertiseAddress);
                    }
                }
            }
            else if (serviceAddresses.Count > 1)
            {
                serviceLogger?.CannotUseMultipleServiceIPAddresses();
                return default;
            }

            var service = new ServiceRegistration(name, serviceAddresses.FirstOrDefault()) { Id = serviceId };
            if (instanceRegistration.InstanceId?.Length > 0)
            {
                // If the identifier of the instance on which the container is running is
                // determined, we will add an additional tag to the service that will allow the
                // IP address of that specific container to be resolved, rather than the entire
                // group with the same service name.
                service.Tags = [instanceRegistration.InstanceId];
            }

            service.Metadata.Add(ContainerAttributeName, container.Id);
            return service;
        }

        private static bool ContainsServiceId(string serviceId, IEnumerable<ServiceRegistration> services)
        {
            return services.Select(service => service.Id).Contains(serviceId, StringComparer.OrdinalIgnoreCase);
        }
    }
}
