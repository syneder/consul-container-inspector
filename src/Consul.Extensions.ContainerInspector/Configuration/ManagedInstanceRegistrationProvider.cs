using Consul.Extensions.ContainerInspector.Configuration.Models;
using System.Collections;
using System.Text.Json;

namespace Consul.Extensions.ContainerInspector.Configuration
{
    public class ManagedInstanceRegistrationProvider(string registrationFilePath, string configurationSection) : ConfigurationProvider
    {
        private bool _isRequired;

        public const string RequireRegistrationEnvironmentName = "MANAGED_INSTANCE_REGISTRATION_REGUIRED";

        /// <summary>
        /// Loads the AWS managed instance registration.
        /// </summary>
        public override void Load()
        {
            LoadEnvironmentVariables(Environment.GetEnvironmentVariables());

            if (Path.Exists(registrationFilePath))
            {
                using var stream = File.OpenRead(registrationFilePath);
                var instanceRegistration = JsonSerializer.Deserialize<ManagedInstanceRegistration>(stream);

                // Although the registration file contains, in addition to the instance ID, the region
                // in which instance is registered, this solution only requires the instance ID, which
                // will be used when registering the service with Consul.
                var name = ConfigurationPath.Combine(configurationSection, nameof(instanceRegistration.InstanceId));
                Data.TryAdd(name, instanceRegistration?.InstanceId);
            }
            else if (_isRequired)
            {
                throw new InvalidOperationException(string.Format(
                    "An {0} environment variable was encountered that indicates that AWS managed " +
                    "instance registration is required, but the registration file '{1}' does not exist.",
                        RequireRegistrationEnvironmentName, registrationFilePath));
            }
        }

        private void LoadEnvironmentVariables(IDictionary envs)
        {
            var enumerator = envs.GetEnumerator();

            try
            {
                while (enumerator.MoveNext())
                {
                    var envName = (string)enumerator.Entry.Key;
                    if (envName.Equals(RequireRegistrationEnvironmentName, StringComparison.OrdinalIgnoreCase))
                    {
                        _isRequired = true;
                        return;
                    }
                }
            }
            finally
            {
                (envs as IDisposable)?.Dispose();
            }
        }
    }
}
