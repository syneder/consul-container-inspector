using System.Buffers;
using System.Text;

namespace Consul.Extensions.ContainerInspector.Configurations
{
    public class ConsulConfigurationProvider(string configurationFilePath, string configurationSection)
        : BaseConfigurationProvider(configurationSection)
    {
        public const string ConfigurationFilePathEnvironmentName = "CONSUL_CONFIG_PATH";
        public const string ConfigurationEnvironmentName = "CONSUL_CONFIG";

        protected override IDictionary<string, string> EnvsMapper => new Dictionary<string, string>
        {
            { ConfigurationFilePathEnvironmentName, string.Empty },
            { ConfigurationEnvironmentName, string.Empty }
        };

        /// <summary>
        /// Loads the Consul configuration files and environment variables.
        /// </summary>
        public override void Load()
        {
            base.Load();

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

        protected override bool LoadFromEnvironmentVariable(string name, string value)
        {
            if (name.Equals(ConfigurationFilePathEnvironmentName, StringComparison.OrdinalIgnoreCase))
            {
                configurationFilePath = value;
                return true;
            }

            // The configuration can be passed via an environment variable in text or Base64.
            // It is preferable to use Base64 format, so try to decode the Base64 string first
            // and then parse the configuration.
            if (name.Equals(ConfigurationEnvironmentName, StringComparison.OrdinalIgnoreCase))
            {
                var size = ((value.Length * 3) + 3) / 4;
                var decodedData = ArrayPool<byte>.Shared.Rent(size);

                try
                {
                    if (Convert.TryFromBase64String(value, decodedData, out var bytesWritten))
                    {
                        value = Encoding.UTF8.GetString(decodedData, 0, bytesWritten);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(decodedData);
                }

                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
                using var streamReader = new StreamReader(stream);
                LoadConfigurations(streamReader);

                return true;
            }

            return default;
        }

        private void LoadConfigurations(StreamReader streamReader)
        {
            foreach (var (configurationPath, configurationData) in new ConsulConfigurationParser(streamReader).Parse())
            {
                TryAdd(configurationPath, configurationData);
            }
        }
    }
}
