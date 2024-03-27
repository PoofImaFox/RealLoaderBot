using System.Reflection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using RealLoaderBot.Discord.Services;
using RealLoaderBot.Services.Interfsces;

namespace RealLoaderBot.Services {
    public class CommandHandlerService : ICommandHandlerService {

        private readonly CommandService _commandService;
        private readonly IServiceProvider _services;
        private readonly IUserRoleWatcherService _userRoleWatcherService;
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _discordSocketClient;

        public CommandHandlerService(DiscordSocketClient discordSocketClient, CommandService commandService,
            IServiceProvider services, IUserRoleWatcherService userRoleWatcherService, ILogger logger) {
            _discordSocketClient = discordSocketClient;
            _commandService = commandService;
            _services = services;
            _userRoleWatcherService = userRoleWatcherService;
            _logger = logger;

            _commandService.Log += CommandServiceLog;
            _commandService.CommandExecuted += CommandServiceCommandExecuted;

            _discordSocketClient.MessageReceived += DiscordSocketClientMessageReceived;
            _discordSocketClient.SlashCommandExecuted += SlashCommandHandler;
        }

        public async Task InitializeAsync() {
            await _userRoleWatcherService.WatchRoles();

            var executingAssembly = Assembly.GetExecutingAssembly();
            var moduleInfo = await _commandService.AddModulesAsync(executingAssembly, _services);

            _logger.Info($"Added {moduleInfo.Sum(i => i.Commands.Count)} commands from {executingAssembly.GetName()}.");

            var globalCommand = new SlashCommandBuilder();
            globalCommand.WithName("mhelp");
            globalCommand.WithDescription("display text commands");

            await _discordSocketClient.CreateGlobalApplicationCommandAsync(globalCommand.Build());
            _logger.Info($"Added slash commands");
        }

        private async Task SlashCommandHandler(SocketSlashCommand command) {
            await _commandService.ExecuteAsync(null, command.Data.Name, _services);
            await command.RespondAsync("adding slash commands");
        }

        private async Task DiscordSocketClientMessageReceived(SocketMessage socketMessage) {
            if (socketMessage is not IUserMessage userMessage) {
                return;
            }

            // TODO: Add role permissions
            if (socketMessage.Author.Id is not 419721107566755851 and not 336571517322264577) {
                return;
            }

            var argumentPosistion = 0;
            if (!userMessage.HasMentionPrefix(_discordSocketClient.CurrentUser, ref argumentPosistion)) {
                return;
            }

            var context = new CommandContext(_discordSocketClient, userMessage);

            _logger.Debug($"Got Command: {socketMessage.Content.Substring(argumentPosistion)}");
            await _commandService.ExecuteAsync(context, argumentPosistion, _services);
        }

        private async Task CommandServiceCommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext context, IResult commandResult) {
            if (!commandInfo.IsSpecified || commandResult.IsSuccess) {
                return;
            }

            _logger.Error($"[{context.User.Username}::{commandInfo.Value.Name}] error: {commandResult}");
            await context.Channel.SendMessageAsync($"[{context.User.Username}::{commandInfo.Value.Name}] error: {commandResult}");
        }

        private Task CommandServiceLog(LogMessage logMessage) {
            var stringMessage = $"[{logMessage.Source}] {logMessage.Message}";

            switch (logMessage.Severity) {
                case LogSeverity.Info:
                    _logger.Info(stringMessage);
                    break;
                case LogSeverity.Warning:
                    _logger.Warning(stringMessage);
                    break;
                case LogSeverity.Error:
                    _logger.Error(stringMessage);
                    break;
                case LogSeverity.Critical:
                    _logger.Error(stringMessage);
                    break;

                default:
                    _logger.Debug(stringMessage);
                    break;
            };

            return Task.CompletedTask;
        }
    }

}
