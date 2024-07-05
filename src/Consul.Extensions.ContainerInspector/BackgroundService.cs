
using Consul.Extensions.ContainerInspector.Configuration.Models;
using Microsoft.Extensions.Options;

namespace Consul.Extensions.ContainerInspector
{
    public class BackgroundService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly ConsulConfiguration _configuration;
        private readonly ManagedInstanceRegistration _instanceRegistration;

        public BackgroundService(
            IOptions<ConsulConfiguration> configuration,
            IOptions<ManagedInstanceRegistration> instanceRegistration)
        {
            _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
            _instanceRegistration = instanceRegistration?.Value
                ?? throw new ArgumentNullException(nameof(instanceRegistration));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
