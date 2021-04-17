using Discord;
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
        private DiscordSocketClient? _discordClient;
        private readonly ILogger<DiscordService> _logger;
        private readonly DiscordSettings _discordSettings;

        public DiscordService(ILogger<DiscordService> logger, DiscordSettings discordSettings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _discordSettings = discordSettings ?? throw new ArgumentNullException(nameof(discordSettings));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _discordClient = new DiscordSocketClient();

            _discordClient.Log += DiscordLog;

            _logger.LogInformation("Starting discord client");
            try
            {
                await _discordClient.LoginAsync(TokenType.Bot, _discordSettings.Token);
                await _discordClient.StartAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception during client startup");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_discordClient is not null)
            {
                _logger.LogInformation("Shutting down discord client");
                try
                {
                    _discordClient.Log -= DiscordLog;
                    _discordClient.Dispose();
                    await _discordClient.StopAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception during client shutdown");
                }
            }
            
            _discordClient = null;
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

            _logger.Log(level, "Discord log: {Log}", message.Message);
            return Task.CompletedTask;
        }
    }
}
