using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polaris
{
    internal class DiscordService : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<DiscordService> _logger;
        private readonly DiscordSettings _discordSettings;

        public DiscordService(DiscordSocketClient client, ILogger<DiscordService> logger, DiscordSettings discordSettings)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _discordSettings = discordSettings ?? throw new ArgumentNullException(nameof(discordSettings));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client.Log += DiscordLog;

            _logger.LogInformation("Starting discord client");
            try
            {
                await _client.LoginAsync(TokenType.Bot, _discordSettings.Token);
                await _client.StartAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception during client startup");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Shutting down discord client");
            try
            {
                _client.Log -= DiscordLog;
                await _client.StopAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception during client shutdown");
            }
        }

        private Task DiscordLog(LogMessage message)
        {
            var level = message.Severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Debug,
                LogSeverity.Debug => LogLevel.Trace,
                _ => throw new NotImplementedException()
            };

            _logger.Log(level, message.Exception, "Discord log: {Log}", message.Message);
            return Task.CompletedTask;
        }
    }
}
