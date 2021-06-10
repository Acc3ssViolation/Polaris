using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;

        public HelpModule(CommandService commandService)
        {
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        }

        [Command("help")]
        [Summary("Provides info about commands")]
        public async Task DefaultHelp()
        {
            var commands = _commandService.Commands;
            var builder = new StringBuilder();
            foreach (var command in commands)
            {
                builder.Append('`');
                if (!string.IsNullOrWhiteSpace(command.Module.Group))
                    builder.Append($"{command.Module.Group} ");
                builder.Append(command.Name);
                builder.Append('`');
                foreach(var parameter in command.Parameters)
                {
                    builder.Append($" `{parameter.Name}`");
                }
                builder.AppendLine($": {command.Summary}");
            }
            await ReplyAsync(builder.ToString());
        }
    }
}
