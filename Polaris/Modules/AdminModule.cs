using Discord.Commands;
using Polaris.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Modules
{

    [RequirePermission(typeof(AdminPermission), Operation.Read)]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("echo")]
        [Summary("Echoes back a message")]
        public Task EchoAsync([Summary("Text to echo")] string message) => ReplyAsync(message);

        [Command("throw")]
        public Task ExceptionThrowAsync() => throw new IndexOutOfRangeException("Wut");
    }

    //[Group("claims")]
    //public class ClaimsModule : ModuleBase<SocketCommandContext>
    //{
    //    private readonly IClaimManager _claimManager;
    //    private readonly IPermissionManager _permissionManager;

    //    public ClaimsModule(IClaimManager claimManager, IPermissionManager permissionManager)
    //    {
    //        _claimManager = claimManager ?? throw new ArgumentNullException(nameof(claimManager));
    //        _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
    //    }

    //    [Command("get")]
    //    [RequirePermission(typeof(AdminPermission.Claims), Operation.Read)]
    //    public async Task GetClaims(ClaimType type, string name)
    //    {
            
    //    }
    //}
}
