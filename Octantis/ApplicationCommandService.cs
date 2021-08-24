using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Octantis.Discord.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Octantis
{
    public class ApplicationCommandService : IHostedService
    {
        private readonly IGateway _gateway;
        private readonly ILogger<ApplicationCommandService> _logger;
        private readonly RestApi _api;

        private IDisposable? _handlers;

        public ApplicationCommandService(IGateway gateway, ILogger<ApplicationCommandService> logger, RestApi api)
        {
            _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _api = api ?? throw new ArgumentNullException(nameof(api));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _handlers = _gateway.AddEventHandler(Event.GuildCreate, async (Guild guild) => 
            {
                var sb = new StringBuilder();
                sb.AppendLine($"New guild '{guild.Name}'");
                sb.AppendLine("Channel list");
                foreach (var channel in guild.Channels)
                {
                    sb.AppendLine($"\t'{channel.Name}', type '{channel.Type}'");
                }

                var members = await _api.GetAsync<GuildMember[]>($"/guilds/{guild.Id}/members?limit=500", cancellationToken);
                sb.AppendLine("User list");
                if (members is not null)
                {
                    foreach (var member in members)
                    {
                        var user = member.User;
                        if (user is not null)
                            sb.AppendLine($"\t'{user.Username}#{user.Discriminator}', joined at '{member.JoinedAt}'");
                        else
                            sb.AppendLine($"\tUNKNOWN USER, joined at '{member.JoinedAt}'");
                    }
                }
                else
                    sb.AppendLine("\t-");

                _logger.LogInformation(sb.ToString());
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _handlers?.Dispose();
            _handlers = null;
        }
    }
}
