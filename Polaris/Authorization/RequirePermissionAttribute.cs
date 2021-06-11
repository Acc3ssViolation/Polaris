using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Polaris.Authorization
{
    public class RequirePermissionAttribute : PreconditionAttribute
    {
        public bool AlwaysAllowOwner { get; }

        public RequirePermissionAttribute(bool alwaysAllowOwner = true)
        {
            AlwaysAllowOwner = alwaysAllowOwner;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var claimProvider = services.GetRequiredService<IClaimProvider>();
            var claims = await claimProvider.GetClaimCollectionAsync(context.User);
            var permissionName = command.Module.Group + "." + command.Name.Replace(' ', '.');

            var allowed = ClaimChecker.IsAllowed(claims, permissionName, true);
            if (allowed)
                return PreconditionResult.FromSuccess();

            if (context.User is SocketGuildUser guildUser)
            {
                if (AlwaysAllowOwner && guildUser.Guild.OwnerId == guildUser.Id)
                    return PreconditionResult.FromSuccess();
            }

            return PreconditionResult.FromError($"User does not have permission to run this command");
        }
    }
}
