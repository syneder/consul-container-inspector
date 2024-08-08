namespace Consul.Extensions.ContainerInspector
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var appBuilder = new HostBuilder()
                .UseServiceProviderFactory(_ => new DefaultServiceProviderFactory())
                .ConfigureServices(ConfigureServices)
                .ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddEnvironmentVariables("INSPECTOR_");

                    configurationBuilder.AddCommandLine(args, new Dictionary<string, string>
                    {
                        { "--consul:address", "consul:advertise_addr" },
                        { "--consul:socket", "consul:addresses:socketPath" },
                        { "--consul:token", "consul:acl:token" },
                        { "--docker:socket", "consul:socketPath" },
                    });

                    configurationBuilder.AddServiceConfigurations();
                });

            return appBuilder.Build().RunAsync();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<ConsoleLifetimeOptions>(options =>
            {
                options.SuppressStatusMessages = true;
            });

            services.AddLogging(serviceLogging =>
            {
                serviceLogging.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.IncludeScopes = true;
                });

                if (context.Configuration.GetSection("verbose").Exists())
                {
                    serviceLogging.SetMinimumLevel(LogLevel.Trace);
                }
                else
                {
                    if (context.Configuration.GetSection("debug").Exists())
                    {
                        serviceLogging.SetMinimumLevel(LogLevel.Debug);
                    }

                    serviceLogging.AddFilter("Microsoft.Extensions.Http.DefaultHttpClientFactory", LogLevel.None);
                }
            });

            services
                .AddCoreServices()
                .BindServiceConfigurations()
                .AddHostedService<BackgroundService>();
        }
    }
}
