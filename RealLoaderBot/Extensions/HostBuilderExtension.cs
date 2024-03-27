using Microsoft.Extensions.Hosting;

using RealLoaderBot.Interfaces;

namespace RealLoaderBot.Extensions {
    public static class HostBuilderExtension {
        public static IHostBuilder UseStartup<T>(this IHostBuilder hostBuilder) where T : IStartup, new() {
            IStartup startup = new T();

            hostBuilder.ConfigureAppConfiguration(startup.Configure);

            hostBuilder.ConfigureAppConfiguration((_, config) => startup.Configuration = config.Build());
            hostBuilder.ConfigureServices(startup.ConfigureServices);

            return hostBuilder;
        }
    }
}
