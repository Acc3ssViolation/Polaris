using Discord.Commands;
using Polaris.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Modules
{

    [RequirePermission(typeof(AdminPermission), Operation.Get)]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("echo")]
        [Summary("Echoes back a message")]
        public Task EchoAsync([Summary("Text to echo")] string message) => ReplyAsync(message);

        [Command("throw")]
        [Summary("Throws an exception")]
        public Task ExceptionThrowAsync() => throw new IndexOutOfRangeException("Wut");
    }

    [Group("perm")]
    [Summary("Provides permission management")]
    public class ClaimsModule : ModuleBase<SocketCommandContext>
    {
        private readonly IClaimManager _claimManager;
        //private readonly IPermissionManager _permissionManager;
        private readonly CommandService _commandService;

        public ClaimsModule(IClaimManager claimManager, CommandService commandService)
        {
            _claimManager = claimManager ?? throw new ArgumentNullException(nameof(claimManager));
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        }

        [Command("list")]
        [Summary("Lists all available permissions")]
        public async Task ListPermissions()
        {

        }

        [Command("command")]
        [Summary("Gets the permission required for a command")]
        public async Task Command(string name)
        {
            var result = _commandService.Search(name);
            if (result.Commands?.Count > 0)
            {
                var command = result.Commands[0].Command;

                var builder = new StringBuilder();
                builder.AppendLine($"Permissions needed for command `{name}`:");
                var permissionAttribute = command.Preconditions.OfType<RequirePermissionAttribute>().FirstOrDefault();
                if (permissionAttribute is null)
                {
                    builder.AppendLine("No permissions required");
                }
                else
                {
                    builder.AppendLine($"`{permissionAttribute.Permission}: {permissionAttribute.Operation}`");
                }
                await ReplyAsync(builder.ToString());
            }
            else
            {
                await ReplyAsync($"Unable to find command {name}");
            }
        }

        [Command("get")]
        [Summary("Gets all permissions for a user or role")]
        [RequirePermission(typeof(AdminPermission.Claims), Operation.Get)]
        public async Task GetClaims(ClaimType type, string name)
        {
            switch (type)
            {
                case ClaimType.User:
                    await GetUserClaims(name);
                    break;
                case ClaimType.Role:
                    await GetRoleClaims(name);
                    break;
                default:
                    await ReplyAsync($"Unknown claim type {type}");
                    break;
            }
        }

        [Command("set")]
        [Summary("Sets a permission for a user or role")]
        [RequirePermission(typeof(AdminPermission.Claims), Operation.Set)]
        public async Task SetClaim(ClaimType type, string name, string identifier, Operation allowedOperations)
        {
            if (allowedOperations == Operation.None)
            {
                await ReplyAsync("To remove a permission use the `rm` command");
                return;
            }

            switch (type)
            {
                case ClaimType.User:
                    await SetUserClaim(name, identifier, allowedOperations);
                    break;
                case ClaimType.Role:
                    await SetRoleClaim(name, identifier, allowedOperations);
                    break;
                default:
                    await ReplyAsync($"Unknown claim type {type}");
                    break;
            }
        }

        [Command("rm")]
        [Summary("Removes a permission from a user or role")]
        [RequirePermission(typeof(AdminPermission.Claims), Operation.Delete)]
        public async Task RemoveClaim(ClaimType type, string name, string identifier)
        {
            switch (type)
            {
                case ClaimType.User:
                    await SetUserClaim(name, identifier, Operation.None);
                    break;
                case ClaimType.Role:
                    await SetRoleClaim(name, identifier, Operation.None);
                    break;
                default:
                    await ReplyAsync($"Unknown claim type {type}");
                    break;
            }
        }

        private async Task SetUserClaim(string name, string identifier, Operation allowedOperations)
        {
            var id = Context.Guild.Users.FirstOrDefault(u => string.Equals(u.Username, name, StringComparison.OrdinalIgnoreCase))?.Id;
            if (id is null)
            {
                await ReplyAsync($"Unable to find user named {name}").ConfigureAwait(false);
                return;
            }

            var claim = new PermissionClaim(identifier, allowedOperations);

            var result = await _claimManager.UpdatePermissionClaimAsync(Context.Guild.Id, ClaimType.User, id.Value, claim, default).ConfigureAwait(false);

            if (result)
            {
                await ReplyAsync($"Set user {name} permission for {identifier} to {allowedOperations}");
            }
            else
            {
                await ReplyAsync($"Failed to update permission");
            }
        }

        private async Task SetRoleClaim(string name, string identifier, Operation allowedOperations)
        {
            var id = Context.Guild.Roles.FirstOrDefault(u => string.Equals(u.Name, name, StringComparison.OrdinalIgnoreCase))?.Id;
            if (id is null)
            {
                await ReplyAsync($"Unable to find role named {name}").ConfigureAwait(false);
                return;
            }

            var claim = new PermissionClaim(identifier, allowedOperations);

            var result = await _claimManager.UpdatePermissionClaimAsync(Context.Guild.Id, ClaimType.Role, id.Value, claim, default).ConfigureAwait(false);

            if (result)
            {
                await ReplyAsync($"Set role {name} permission for {identifier} to {allowedOperations}");
            }
            else
            {
                await ReplyAsync($"Failed to update permission");
            }
        }

        private async Task GetUserClaims(string name)
        {
            var id = Context.Guild.Users.FirstOrDefault(u => string.Equals(u.Username, name, StringComparison.OrdinalIgnoreCase))?.Id;
            if (id is null)
            {
                await ReplyAsync($"Unable to find user named {name}").ConfigureAwait(false);
                return;
            }

            var result = await _claimManager.GetUserClaimCollectionAsync(Context.Guild.Id, id.Value, default).ConfigureAwait(false);
            await ReplyClaimCollection(name, result).ConfigureAwait(false);
        }

        private async Task GetRoleClaims(string name)
        {
            var id = Context.Guild.Roles.FirstOrDefault(u => string.Equals(u.Name, name, StringComparison.OrdinalIgnoreCase))?.Id;
            if (id is null)
            {
                await ReplyAsync($"Unable to find role named {name}").ConfigureAwait(false);
                return;
            }

            var result = await _claimManager.GetRoleClaimCollectionAsync(Context.Guild.Id, id.Value, default).ConfigureAwait(false);
            await ReplyClaimCollection(name, result).ConfigureAwait(false);
        }

        private async Task ReplyClaimCollection(string prefix, IClaimCollection? collection)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Claims for {prefix}");
            if (collection is null)
                builder.AppendLine("No claims found");
            else
            {
                builder.AppendLine("```");

                foreach (var claim in collection.Claims)
                {
                    builder.AppendLine($"{claim.Identifier}: {claim.ClaimedOperations}");
                }

                builder.AppendLine("```");
            }

            await ReplyAsync(builder.ToString()).ConfigureAwait(false);
        }
    }
}
