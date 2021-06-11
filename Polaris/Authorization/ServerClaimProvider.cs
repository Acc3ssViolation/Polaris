using Discord;
using Discord.WebSocket;
using Polaris.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    internal class ServerClaimProvider : IClaimProvider
    {
        private readonly IClaimManager _claimManager;

        public ServerClaimProvider(IClaimManager claimManager)
        {
            _claimManager = claimManager ?? throw new ArgumentNullException(nameof(claimManager));
        }

        public async Task<IClaimCollection> GetClaimCollectionAsync(IUser user)
        {
            if (user is SocketGuildUser guildUser)
            {
                var claims = new List<Claim>();
                var guildId = guildUser.Guild.Id;
                var claimCollection = await _claimManager.GetClaimCollectionAsync(new GuildSubject(SubjectType.User, user.Id, guildId), default).ConfigureAwait(false);

                claims.AddRange(claimCollection!.Claims);

                foreach (var role in guildUser.Roles)
                {
                    claimCollection = await _claimManager.GetClaimCollectionAsync(new GuildSubject(SubjectType.Role, role.Id, guildId), default).ConfigureAwait(false);
                    claims.AddRange(claimCollection!.Claims);
                }

                return new ClaimCollection(new GuildSubject(SubjectType.User, user.Id, guildId), claims);
            }

            return new ClaimCollection(new GuildSubject(SubjectType.User, user.Id, 0), Array.Empty<Claim>());
        }
    }
}
