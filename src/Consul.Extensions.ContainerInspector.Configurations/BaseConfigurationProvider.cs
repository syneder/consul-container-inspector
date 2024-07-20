using Microsoft.Extensions.Configuration;
using System.Collections;

namespace Consul.Extensions.ContainerInspector.Configurations
{
    /// <summary>
    /// Base class for implementing <see cref="IConfigurationProvider"/> using an environment variables mapper.
    /// </summary>
    public abstract class BaseConfigurationProvider(string configurationSection) : ConfigurationProvider
    {
        /// <summary>
        /// Gets the mapper to map the environment variable name to the configuration path.
        /// </summary>
        protected abstract IDictionary<string, string> EnvsMapper { get; }

        /// <summary>
        /// Gets the separator of an array of values. 
        /// </summary>
        protected virtual char Separator => ',';

        public override void Load()
        {
            LoadFromEnvironmentVariables(Environment.GetEnvironmentVariables());
        }

        protected virtual bool LoadFromEnvironmentVariable(string name, string value)
        {
            if (EnvsMapper.TryGetValue(name, out string? configurationPath))
            {
                var sectionConfigurationPath = ConfigurationPath.GetSectionKey(configurationPath);
                if (sectionConfigurationPath.StartsWith('[') && sectionConfigurationPath.EndsWith(']'))
                {
                    // If the configuration name is enclosed in square brackets, the configuration value
                    // will be split using Separator and written as an array of values. Square brackets
                    // will be removed from the configuration name.
                    sectionConfigurationPath = sectionConfigurationPath.TrimStart('[').TrimEnd(']');

                    var parentConfigurationPath = ConfigurationPath.GetParentPath(configurationPath);
                    if (parentConfigurationPath == default)
                    {
                        configurationPath = sectionConfigurationPath;
                    }
                    else
                    {
                        configurationPath = ConfigurationPath.Combine(
                            parentConfigurationPath, sectionConfigurationPath);
                    }

                    var index = 0;
                    foreach (var data in value.Split(Separator).Select(value => value.Trim()))
                    {
                        if (TryAdd(ConfigurationPath.Combine(configurationPath, index.ToString()), value))
                        {
                            index++;
                        }
                    }

                    return true;
                }

                return TryAdd(configurationPath, value);
            }

            return default;
        }

        protected bool TryAdd(string configurationPath, string value)
        {
            // Ignore duplicate environment variable names and return true to increase the
            // number of successful additions to abort further searches after all matches have
            // been applied.
            return Data.TryAdd(ConfigurationPath.Combine(configurationSection, configurationPath), value);
        }

        /// <summary>
        /// Load the configuration from the environment variables.
        /// </summary>
        private void LoadFromEnvironmentVariables(IDictionary envs)
        {
            var enumerator = envs.GetEnumerator();
            var counter = 0;

            try
            {
                while (enumerator.MoveNext() && counter < EnvsMapper.Count)
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

                    if (LoadFromEnvironmentVariable(name, value.Trim()))
                    {
                        counter++;
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
