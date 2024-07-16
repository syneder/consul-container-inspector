using Microsoft.Extensions.Configuration;
using System.Buffers;
using System.Collections;
using System.Text;

namespace Consul.Extensions.ContainerInspector.Core.Configuration
{
    public class ConsulConfigurationProvider(string configurationFilePath, string configurationSection) : ConfigurationProvider
    {
        public const string ConfigurationFilePathEnvironmentName = "CONSUL_CONFIG_PATH";
        public const string ConfigurationEnvironmentName = "CONSUL_CONFIG";

        /// <summary>
        /// Loads the Consul configuration files and environment variables.
        /// </summary>
        public override void Load()
        {
            LoadFromEnvironmentVariables(Environment.GetEnvironmentVariables());

            // Note that the LoadFromEnvironmentVariables method can change the configuration path
            // by obtaining a new value from an environment variable. Additionally, the configuration
            // path can be a file or a folder. If it's a folder, we need to find all the .hcl files in
            // this folder and read them all.
            if (Path.Exists(configurationFilePath))
            {
                var paths = new Queue<string>([configurationFilePath]);
                while (paths.TryDequeue(out var currentPath))
                {
                    var currentPathAttributes = File.GetAttributes(currentPath);
                    if (currentPathAttributes.HasFlag(FileAttributes.Directory))
                    {
                        foreach (var path in Directory.EnumerateFiles(currentPath, "*.hcl", SearchOption.TopDirectoryOnly))
                        {
                            paths.Enqueue(path);
                        }

                        continue;
                    }

                    using var openedStream = File.OpenRead(currentPath);
                    using var stream = new BufferedStream(openedStream, 1024);
                    using var streamReader = new StreamReader(stream);
                    LoadConfigurations(streamReader);
                }
            }
        }

        private void LoadFromEnvironmentVariables(IDictionary envs)
        {
            var enumerator = envs.GetEnumerator();

            try
            {
                while (enumerator.MoveNext())
                {
                    var envValue = (string?)enumerator.Entry.Value;
                    if (string.IsNullOrWhiteSpace(envValue))
                    {
                        continue;
                    }

                    var envName = (string)enumerator.Entry.Key;
                    if (envName.Equals(ConfigurationFilePathEnvironmentName, StringComparison.OrdinalIgnoreCase))
                    {
                        configurationFilePath = envValue;
                        continue;
                    }

                    // The configuration can be passed via an environment variable in text or Base64.
                    // It is preferable to use Base64 format, so try to decode the Base64 string first
                    // and then parse the configuration.
                    if (envName.Equals(ConfigurationEnvironmentName, StringComparison.OrdinalIgnoreCase))
                    {
                        var size = ((envValue.Length * 3) + 3) / 4;
                        var decodedData = ArrayPool<byte>.Shared.Rent(size);

                        try
                        {
                            if (Convert.TryFromBase64String(envValue, decodedData, out var bytesWritten))
                            {
                                envValue = Encoding.UTF8.GetString(decodedData, 0, bytesWritten);
                            }
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(decodedData);
                        }

                        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(envValue));
                        using var streamReader = new StreamReader(stream);
                        LoadConfigurations(streamReader);
                    }
                }
            }
            finally
            {
                (envs as IDisposable)?.Dispose();
            }
        }

        private void LoadConfigurations(StreamReader streamReader)
        {
            foreach (var data in new ConsulConfigurationParser(streamReader).Parse())
            {
                var configurationPath = ConfigurationPath.Combine(configurationSection, data.Key);
                Data.TryAdd(configurationPath, data.Value);
            }
        }
    }
}
