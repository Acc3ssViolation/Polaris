using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Polaris.Authorization
{
    public class RequirePermissionAttribute : PreconditionAttribute
    {
        public bool InheritParentPermission { get; }
        public bool AlwaysAllowOwner { get; }

        public RequirePermissionAttribute(bool inheritParentPermission = true, bool alwaysAllowOwner = false)
        {
            InheritParentPermission = inheritParentPermission;
            AlwaysAllowOwner = alwaysAllowOwner;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var claimProvider = services.GetRequiredService<IClaimProvider>();
            var claims = await claimProvider.GetClaimCollectionAsync(context.User);

            if (context.User is SocketGuildUser guildUser)
            {
                if (AlwaysAllowOwner && guildUser.Guild.OwnerId == guildUser.Id)
                    return PreconditionResult.FromSuccess();
            }

            var permissionName = command.Module.Group + "." + command.Name.Replace(' ', '.');

            do
            {
                Console.WriteLine($"Checking permission {permissionName}");
                if (claims.HasClaim(permissionName))
                    return PreconditionResult.FromSuccess();

                var parentNameEndIndex = permissionName.IndexOf('.');
                if (parentNameEndIndex == -1)
                    break;

                permissionName = permissionName.Substring(0, parentNameEndIndex);
            } while (InheritParentPermission);

            return PreconditionResult.FromError($"User does not have permission to run this command");
        }
    }
}
