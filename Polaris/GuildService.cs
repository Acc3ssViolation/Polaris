using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polaris.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polaris
{
    public class GuildService : IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly Func<PolarisDbContext> _dbFactory;
        private readonly ILogger<GuildService> _logger;

        public GuildService(DiscordSocketClient client, Func<PolarisDbContext> dbFactory, ILogger<GuildService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _client.GuildAvailable += JoinedGuildAsync;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _client.GuildAvailable -= JoinedGuildAsync;

            return Task.CompletedTask;
        }

        private async Task JoinedGuildAsync(SocketGuild guild)
        {
            using var dbContext = _dbFactory();

            var dbServer = await dbContext.Servers.FirstOrDefaultAsync(s => s.Id == guild.Id).ConfigureAwait(false);
            if (dbServer is null)
            {
                dbServer = new DbServer
                {
                    Id = guild.Id,
                    Name = guild.Name,
                };
                dbContext.Servers.Add(dbServer);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                _logger.LogInformation("Joined new server {id} ({name})", guild.Id, guild.Name);
            }
        }
    }
}
