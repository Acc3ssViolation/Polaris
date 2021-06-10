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
        public Task TestAsync(SocketUser targetUser) => ReplyAsync($"Account was created on {targetUser.CreatedAt}");

        [Command("created-at")]
        public Task TestAsync(SocketRole targetRole) => ReplyAsync($"Role was created on {targetRole.CreatedAt}");
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

        [Command("add")]
        [RequirePermission]
        public async Task AddAsync(SocketGuildUser user, string permission)
        {
            if (!DoesPermissionExist(permission))
            {
                await ReplyAsync("Permission does not exist");
                return;
            }
            await _claimManager.SetPermissionClaimAsync(GuildSubject.FromGuildUser(user), permission, default);
            await ReplyAsync("Permission added to user");
        }

        [Command("add")]
        [RequirePermission]
        public async Task AddAsync(SocketRole role, string permission)
        {
            if (!DoesPermissionExist(permission))
            {
                await ReplyAsync("Permission does not exist");
                return;
            }
            await _claimManager.SetPermissionClaimAsync(GuildSubject.FromRole(role), permission, default);
            await ReplyAsync("Permission added to role");
        }


        [Command("del")]
        [RequirePermission]
        public async Task DeleteAsync(SocketGuildUser user, string permission)
        {
            await _claimManager.DeletePermissionClaimAsync(GuildSubject.FromGuildUser(user), permission, default);
            await ReplyAsync("Permission removed from user");
        }

        [Command("del")]
        [RequirePermission]
        public async Task DeleteAsync(SocketRole role, string permission)
        {
            await _claimManager.DeletePermissionClaimAsync(GuildSubject.FromRole(role), permission, default);
            await ReplyAsync("Permission removed from role");
        }

        [Command("exists")]
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
