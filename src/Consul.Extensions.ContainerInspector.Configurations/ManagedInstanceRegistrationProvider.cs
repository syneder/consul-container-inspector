using Consul.Extensions.ContainerInspector.Configurations.Models;
using System.Text.Json;

namespace Consul.Extensions.ContainerInspector.Configurations
{
    public class ManagedInstanceRegistrationProvider(string registrationFilePath, string configurationSection)
        : BaseConfigurationProvider(configurationSection)
    {
        private bool _isRequired;

        public const string RequireRegistrationEnvironmentName = "MANAGED_INSTANCE_REGISTRATION_REGUIRED";
        public const string RegistrationFilePathEnvironmentName = "MANAGED_INSTANCE_REGISTRATION_FILE_PATH";

        protected override IDictionary<string, string> EnvsMapper => new Dictionary<string, string>
        {
            { RequireRegistrationEnvironmentName, string.Empty },
            { RegistrationFilePathEnvironmentName, string.Empty },
        };

        /// <summary>
        /// Loads the Consul configuration files and environment variables.
        /// </summary>
        public override void Load()
        {
            base.Load();

            if (Path.Exists(registrationFilePath))
            {
                using var stream = File.OpenRead(registrationFilePath);
                var instanceRegistration = JsonSerializer.Deserialize<ManagedInstanceRegistration>(stream);

                // Although the registration file contains, in addition to the instance ID, the region
                // in which instance is registered, this solution only requires the instance ID, which
                // will be used when registering the service with Consul.
                TryAdd(nameof(instanceRegistration.InstanceId), instanceRegistration?.InstanceId);
            }
            else if (_isRequired)
            {
                throw new InvalidOperationException(string.Format(
                    "An {0} environment variable was encountered that indicates that AWS managed " +
                    "instance registration is required, but the registration file '{1}' does not exist.",
                        RequireRegistrationEnvironmentName, registrationFilePath));
            }
        }

        protected override bool LoadFromEnvironmentVariable(string name, string value)
        {
            if (name.Equals(RequireRegistrationEnvironmentName, StringComparison.OrdinalIgnoreCase))
            {
                _isRequired = true;
                return true;
            }

            if (name.Equals(RegistrationFilePathEnvironmentName, StringComparison.OrdinalIgnoreCase))
            {
                registrationFilePath = value;
                return true;
            }

            return default;
        }
    }
}
