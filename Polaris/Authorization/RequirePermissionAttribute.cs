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
        public string Permission { get; }
        public Operation Operation { get; }
        public bool AlwaysAllowOwner { get; }

        public RequirePermissionAttribute(string permission, Operation operation, bool alwaysAllowOwner = true)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
            Operation = operation;
            AlwaysAllowOwner = alwaysAllowOwner;
        }

        public RequirePermissionAttribute(Type permission, Operation operation, bool alwaysAllowOwner = true) 
        {
            var permissionInstance = Activator.CreateInstance(permission) as IPermission;
            Permission = permissionInstance?.Identifier ?? throw new ArgumentException($"{permission.GetType()} is not an IPermission");
            Operation = operation;
            AlwaysAllowOwner = alwaysAllowOwner;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var claimProvider = services.GetRequiredService<IClaimProvider>();
            var claims = await claimProvider.GetClaimCollectionAsync(context.User);
            if (!claims.HasClaim(Permission, Operation))
            {
                if (context.User is SocketGuildUser guildUser)
                {
                    if (guildUser.Guild.OwnerId == guildUser.Id)
                        return PreconditionResult.FromSuccess();
                }

                return PreconditionResult.FromError($"User needs permission `{Permission} ({Operation})` to run this command");
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
