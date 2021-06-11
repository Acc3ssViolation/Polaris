using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Polaris.Authorization;
using Polaris.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Modules
{

    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("echo")]
        [Summary("Echoes back a message")]
        public Task EchoAsync([Summary("Text to echo")] string message) => ReplyAsync(message);

        [Command("throw")]
        [Summary("Throws an exception")]
        public Task ExceptionThrowAsync() => throw new IndexOutOfRangeException("Wut");

        [Command("created-at")]
        public Task TestAsync(SocketUser targetUser) => ReplyAsync($"User <@{targetUser.Id}> was created on {targetUser.CreatedAt}", allowedMentions: AllowedMentions.None);

        [Command("created-at")]
        public Task TestAsync(SocketRole targetRole) => ReplyAsync($"Role <@{targetRole.Id}> was created on {targetRole.CreatedAt}", allowedMentions: AllowedMentions.None);
    }

    [Group("perm")]
    [Summary("Provides permission management")]
    public class ClaimsModule : ModuleBase<SocketCommandContext>
    {
        private readonly IClaimManager _claimManager;
        private readonly IClaimProvider _claimProvider;
        private readonly CommandService _commandService;

        public ClaimsModule(IClaimManager claimManager, IClaimProvider claimProvider, CommandService commandService)
        {
            _claimManager = claimManager ?? throw new ArgumentNullException(nameof(claimManager));
            _claimProvider = claimProvider ?? throw new ArgumentNullException(nameof(claimProvider));
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        }

        [Command("check")]
        [Summary("Checks if a certain user is allowed a certain permission when taking into account all their roles and permission inheritance rules.")]
        [RequirePermission]
        public async Task IsAllowed(SocketGuildUser user, string permission)
        {
            var subject = GuildSubject.FromGuildUser(user);
            var claims = await _claimProvider.GetClaimCollectionAsync(user);
            var allowed = ClaimChecker.IsAllowed(claims!, permission, true);

            if (allowed)
                await ReplyAsync($"{subject.MentionString} is allowed permission `{permission}`", allowedMentions: AllowedMentions.None);
            else
                await ReplyAsync($"{subject.MentionString} is not allowed permission `{permission}`", allowedMentions: AllowedMentions.None);
        }

        [Command("show")]
        [Summary("Shows permissions that are configured for a user.")]
        [RequirePermission]
        public async Task ListAsync(SocketGuildUser user)
        {
            var claims = await _claimManager.GetClaimCollectionAsync(GuildSubject.FromGuildUser(user), default);

            await ReplyPermissions(claims!);
        }

        [Command("show")]
        [Summary("Shows permissions that are configured for a role.")]
        [RequirePermission]
        public async Task ListAsync(SocketRole role)
        {
            var claims = await _claimManager.GetClaimCollectionAsync(GuildSubject.FromRole(role), default);

            await ReplyPermissions(claims!);
        }

        private Task ReplyPermissions(IClaimCollection claims)
        {
            var b = new StringBuilder();
            b.AppendLine($"The following permissions are set for {claims.Subject.MentionString}:");
            if (claims!.Claims.Count > 0)
            {
                var maxWidth = claims.Claims.Max(c => c.Identifier.Length);
                foreach (var claim in claims.Claims)
                {
                    b.AppendLine($"`{claim.Identifier.PadRight(maxWidth)}   {claim.Allow}`");
                }
            }
            else
            {
                b.AppendLine("`No persmissions configured`");
            }

            return ReplyAsync(b.ToString(), allowedMentions: AllowedMentions.None);
        }


        [Command("set")]
        [Summary("Configures a permission for a user. Permissions can be allowed by setting them to `true` or explicitly forbidden by setting them to `false`.")]
        [RequirePermission]
        public async Task AddAsync(SocketGuildUser user, string permission, bool value)
        {
            if (!DoesPermissionExist(permission))
            {
                await ReplyAsync("Permission does not exist");
                return;
            }
            await _claimManager.SetPermissionClaimAsync(GuildSubject.FromGuildUser(user), new Claim(permission, value), default);
            await ReplyAsync("Permission added to user");
        }

        [Command("set")]
        [Summary("Configures a permission for a role. Permissions can be allowed by setting them to `true` or explicitly forbidden by setting them to `false`.")]
        [RequirePermission]
        public async Task AddAsync(SocketRole role, string permission, bool value)
        {
            if (!DoesPermissionExist(permission))
            {
                await ReplyAsync("Permission does not exist");
                return;
            }
            await _claimManager.SetPermissionClaimAsync(GuildSubject.FromRole(role), new Claim(permission, value), default);
            await ReplyAsync("Permission added to role");
        }


        [Command("del")]
        [Summary("Deletes a permission for a user.")]
        [RequirePermission]
        public async Task DeleteAsync(SocketGuildUser user, string permission)
        {
            await _claimManager.DeletePermissionClaimAsync(GuildSubject.FromGuildUser(user), permission, default);
            await ReplyAsync("Permission removed from user");
        }

        [Command("del")]
        [Summary("Deletes a permission for a role.")]
        [RequirePermission]
        public async Task DeleteAsync(SocketRole role, string permission)
        {
            await _claimManager.DeletePermissionClaimAsync(GuildSubject.FromRole(role), permission, default);
            await ReplyAsync("Permission removed from role");
        }

        [Command("exists")]
        [Summary("Checks if a given permission name exists and can be used.")]
        [RequirePermission]
        public async Task Exists(string permission)
        {
            if (DoesPermissionExist(permission))
            {
                await ReplyAsync("Permission exists");
            }
            else
            {
                await ReplyAsync("Permission does not exist");
            }
        }

        private bool DoesPermissionExist(string permission)
        {
            var split = permission.Split('.');

            // Special case, no nesting
            if (split.Length == 1)
            {
                if (_commandService.Commands.Any(c => c.Name == split[0]))
                    return true;

                if (_commandService.Modules.Any(s => s.Group == split[0]))
                    return true;

                return false;
            }

            // Check modules
            var modules = _commandService.Modules;
            for (int i = 0; i < split.Length - 1; i++)
            {
                var matchingModule = modules.FirstOrDefault(m => m.Group == split[i]);
                if (matchingModule is null)
                {
                    return false;
                }

                if (i == split.Length - 2)
                {
                    // Last module, next is the command name
                    return matchingModule.Commands.Any(c => c.Name == split[^1]);
                }

                modules = matchingModule.Submodules;
            }

            throw new NotImplementedException();
        }

        private static bool HasCommandOrSubModule(ModuleInfo module, string name)
        {
            if (module.Commands.Any(c => c.Name == name))
                return true;

            if (module.Submodules.Any(s => s.Group == name))
                return true;

            return false;
        }
    }
}
