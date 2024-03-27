using System.Reflection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RealLoaderBot.Config;
using RealLoaderBot.Discord;
using RealLoaderBot.Discord.Services;
using RealLoaderBot.Interfaces;
using RealLoaderBot.Models.Config;
using RealLoaderBot.Services;
using RealLoaderBot.Services.Discord.Interfaces;
using RealLoaderBot.Services.Interfsces;

namespace RealLoaderBot {
    public class Startup : IStartup {
        public IConfiguration Configuration { get; set; } = default!;

        public void Configure(HostBuilderContext hostBuilderContext, IConfigurationBuilder configurationBuilder) {
            configurationBuilder
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .AddEnvironmentVariables();
        }

        public void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services) {
            var socketClient = new DiscordSocketClient(new DiscordSocketConfig {
                LogGatewayIntentWarnings = true,
                LogLevel = LogSeverity.Verbose
            });

            var discordConfig = new DiscordUserConfig(Configuration);
            socketClient.LoginAsync(TokenType.Bot, discordConfig.DiscordToken).Wait();
            socketClient.StartAsync().Wait();

            Console.WriteLine("Waiting for login");
            while (socketClient.CurrentUser is null) {
                Thread.Sleep(1500);
            }
            Console.WriteLine("Logged in");

            //var pterodactylConfig = new PterodactylConfig(Configuration);

            //services.AddHttpClient(nameof(PterodactylConfig), client => {
            //    client.BaseAddress = new Uri(pterodactylConfig.ApiBaseUrl);
            //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", pterodactylConfig.PterodactylApiKey);
            //}).AddPolicyHandler(GetRetryPolicy());

            services
                .AddSingleton<LoggingConfig>()
                .AddSingleton<RoleWatcherConfig>()
                .AddSingleton(discordConfig);

            services
                .AddSingleton<IDiscordNotificationService, DiscordNotificationService>();

            services
                .AddSingleton<ILogger, Logger>()
                .AddSingleton(socketClient)
                .AddSingleton<IUserRoleWatcherService, UserRoleWatcherService>()
                .AddSingleton<ICommandHandlerService, CommandHandlerService>()
                .AddSingleton(new CommandService());
        }

        //static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() {
        //    return HttpPolicyExtensions
        //        .HandleTransientHttpError()
        //        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        //        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
