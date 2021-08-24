using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly IOptions<DiscordSettings> _settings;

        private IDisposable? _handlers;

        public ApplicationCommandService(IGateway gateway, ILogger<ApplicationCommandService> logger, RestApi api, IOptions<DiscordSettings> settings)
        {
            _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Test guild: '{Id}'", _settings.Value.TestGuildId);
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

                if (guild.Id != _settings.Value.TestGuildId)
                    return;

                // This is the test guild, add a command!
                var helloWorldCommand = new ApplicationCommand
                {
                    Name = "hello",
                    Description = "My first slash command!",
                    Type = ApplicationCommandType.ChatInput
                };
                var result = await _api.PostAsync<ApplicationCommand, ApplicationCommand>($"/applications/{_gateway.ApplicationId}/guilds/{guild.Id}/commands", helloWorldCommand, cancellationToken);
                if (result is not null)
                    _logger.LogInformation("Added application command '{Command}'", result.Name);
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _handlers?.Dispose();
            _handlers = null;
        }
    }
}
