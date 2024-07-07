using Microsoft.Extensions.Configuration;
using System.Collections;

namespace Consul.Extensions.ContainerInspector.Core.Configuration
{
    /// <summary>
    /// Base class for implementing <see cref="IConfigurationProvider"/> using an environment variable mapper.
    /// </summary>
    public abstract class ConfigurationProviderBase(string configurationSection) : ConfigurationProvider
    {
        private readonly Dictionary<string, string> _envsMap = new(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Gets the mapper to map the environment variable name to the configuration path. Mapper
        /// is case insensitive.
        /// </summary>
        protected IDictionary<string, string> EnvironmentVariablesMap => _envsMap;

        public override void Load()
        {
            LoadFromEnvironmentVariables(Environment.GetEnvironmentVariables());
        }

        /// <summary>
        /// Load the configuration from the environment variable.
        /// </summary>
        private void LoadFromEnvironmentVariables(IDictionary envs)
        {
            var enumerator = envs.GetEnumerator();
            var counter = 0;

            try
            {
                while (enumerator.MoveNext() && counter < _envsMap.Count)
                {
                    // If the value of an environment variable contains only spaces, we will treat
                    // it as an unset value. Missing a value does not cause the configuration to be
                    // overwritten with empty values.
                    if (enumerator.Entry.Value is not string value || string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    if (enumerator.Entry.Key is not string name)
                    {
                        continue;
                    }

                    if (_envsMap.TryGetValue(name, out string? configurationPath))
                    {
                        // Ignore duplicate environment variable names and count the number of
                        // successful additions to abort further searches after all matches have
                        // been applied.
                        if (Data.TryAdd(ConfigurationPath.Combine(configurationSection, configurationPath), value))
                        {
                            counter++;
                        }
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
