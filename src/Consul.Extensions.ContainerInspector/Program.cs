using Consul.Extensions.ContainerInspector.Configuration.Models;
using Consul.Extensions.ContainerInspector.Core.Extensions;
using Consul.Extensions.ContainerInspector.Extensions;

namespace Consul.Extensions.ContainerInspector
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var appBuilder = new HostBuilder()
                .UseServiceProviderFactory(_ => new DefaultServiceProviderFactory())
                .ConfigureServices(ConfigureServices)
                .ConfigureServices(services =>
                {
                    services.AddLogging(serviceLogging => ConfigureLogging(serviceLogging, args));
                })
                .ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddEnvironmentVariables("INSPECTOR_");

                    configurationBuilder.AddCommandLine(args, new Dictionary<string, string>
                    {
                        { "--consul:token", "consul:acl:tokens:agent" },
                    });

                    configurationBuilder.AddConsulConfiguration();
                    configurationBuilder.AddDockerConfiguration();
                    configurationBuilder.AddManagedInstanceRegistration();
                });

            return appBuilder.Build().RunAsync();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<ConsulConfiguration>(
                context.Configuration.GetSection(
                    Extensions.ConfigurationExtensions.ConsulConfigurationSection));

            services.Configure<ManagedInstanceRegistration>(
                context.Configuration.GetSection(
                    Extensions.ConfigurationExtensions.ManagedInstanceConfigurationSection));

            services
                .AddDockerInspector(context.Configuration)
                .AddHostedService<BackgroundService>();
        }

        private static void ConfigureLogging(ILoggingBuilder serviceLogging, string[] args)
        {
            if (args.Contains("--debug", StringComparer.OrdinalIgnoreCase))
            {
                serviceLogging.SetMinimumLevel(LogLevel.Debug);
            }

            serviceLogging.AddSystemdConsole(options =>
            {
                options.UseUtcTimestamp = true;
            });

            serviceLogging.Configure(options =>
            {
                options.ActivityTrackingOptions =
                    ActivityTrackingOptions.SpanId |
                    ActivityTrackingOptions.TraceId |
                    ActivityTrackingOptions.ParentId;
            });
        }
    }
}
