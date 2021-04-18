using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polaris
{
    internal class PluginService : IHostedService
    {
        private readonly ILogger<PluginService> _logger;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSettings _settings;

        public PluginService(ILogger<PluginService> logger, DiscordSocketClient client, CommandService commandService, IServiceProvider serviceProvider, DiscordSettings settings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            _client.MessageReceived += OnMessageReceived;
            _commandService.Log += LogAsync;
            _commandService.CommandExecuted += OnCommandExecutedAsync;
            _logger.LogInformation("Started plugin service");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived -= OnMessageReceived;
            _commandService.Log -= LogAsync;
            _commandService.CommandExecuted -= OnCommandExecutedAsync;
            _logger.LogInformation("Stopped plugin service");
        }

        private async Task LogAsync(LogMessage message)
        {
            if (message.Exception is CommandException commandException)
            {
                await commandException.Context.Channel.SendMessageAsync("Beep boop :(");

                _logger.LogError(commandException, "Exception when trying execute process user command");
            }
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!string.IsNullOrEmpty(result.ErrorReason) && result.Error != CommandError.Exception)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }

            var commandName = command.IsSpecified ? command.Value.Name : "A command";
            _logger.LogInformation("{commandName} was executed at {DateTime.UtcNow}.", commandName, DateTime.UtcNow);
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            if (message is not SocketUserMessage userMessage)
                return;

            var argPos = 0;
            if (!userMessage.HasStringPrefix(_settings.CommandPrefix, ref argPos) || userMessage.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, userMessage);

            try
            {
                await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception when trying execute process user command");
            }
        }
    }
}
