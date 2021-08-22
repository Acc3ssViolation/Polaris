using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Octantis
{
    internal class DiscordService : IHostedService
    {
        private readonly IOptions<DiscordSettings> _settings;
        private readonly ILogger<DiscordService> _logger;

        public DiscordService(IOptions<DiscordSettings> settings, ILogger<DiscordService> logger)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Started Discord Service");
            _logger.LogDebug("Mod role: {ModRole}", _settings.Value.ModRole);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopped Discord Service");
            return Task.CompletedTask;
        }
    }
}
