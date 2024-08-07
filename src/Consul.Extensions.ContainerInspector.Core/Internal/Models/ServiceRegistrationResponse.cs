﻿using Consul.Extensions.ContainerInspector.Core.Models;

namespace Consul.Extensions.ContainerInspector.Core.Internal.Models
{
    /// <summary>
    /// Describes the response to an API request to the Consul agent.
    /// </summary>
    internal class ServiceRegistrationResponse() : ServiceRegistration(string.Empty)
    {
        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        /// <remarks>
        /// Has the same meaning as the <see cref="ServiceRegistration.Name" /> property.
        /// </remarks>
        public required string Service
        {
            get => Name;
            set => Name = value;
        }
    }
}
