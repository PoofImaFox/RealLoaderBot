using Microsoft.Extensions.Hosting;

using RealLoaderBot.Extensions;
using RealLoaderBot.Services;
using RealLoaderBot.Services.Discord.Interfaces;

namespace RealLoaderBot {
    internal static class Program {
        static async Task Main(string[] args) {
            Console.WriteLine("Startup");
            var host = new HostBuilder()
                .UseConsoleLifetime()
                .UseStartup<Startup>()
                .Build();

            Console.WriteLine("Init bot");
            // Init our discord bot
            var commandHandler = host.Services.GetService(typeof(ICommandHandlerService)) as ICommandHandlerService;
            await commandHandler.InitializeAsync();

            Console.WriteLine("Starting status");
            // Poll status
            var statusUpdatingService = host.Services.GetService(typeof(IDiscordNotificationService)) as IDiscordNotificationService;
            await statusUpdatingService.StartUpdates();

            Console.WriteLine("Running");
            host.Run();
        }
    }
}
