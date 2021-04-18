using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    internal class ServerClaimProvider : IClaimProvider
    {
        private class EmptyClaimCollection : IClaimCollection
        {
            public ulong GuildId { get; }

            public IReadOnlyCollection<IPermissionClaim> Claims => Array.Empty<IPermissionClaim>();

            public EmptyClaimCollection(ulong guildId)
            {
                GuildId = guildId;
            }
        }

        private class CombinedClaimCollection : IClaimCollection
        {
            public ulong GuildId { get; }

            public IReadOnlyCollection<IPermissionClaim> Claims { get; }

            public CombinedClaimCollection(ulong guildId, IEnumerable<IClaimCollection> collections)
            {
                GuildId = guildId;

                var claims = new Dictionary<string, IPermissionClaim>();
                foreach (var collection in collections)
                {
                    foreach(var claim in collection.Claims)
                    {
                        if (claims.TryGetValue(claim.Identifier, out var exitingClaim))
                        {
                            exitingClaim = new PermissionClaim(claim.Identifier, exitingClaim.ClaimedOperations | claim.ClaimedOperations);
                        }
                        else
                        {
                            exitingClaim = claim;
                        }
                        claims[claim.Identifier] = exitingClaim;
                    }
                }

                Claims = claims.Values.ToList();
            }
        }

        private readonly IClaimManager _claimManager;

        public ServerClaimProvider(IClaimManager claimManager)
        {
            _claimManager = claimManager ?? throw new ArgumentNullException(nameof(claimManager));
        }

        public async Task<IClaimCollection> GetClaimCollectionAsync(IUser user)
        {
            if (user is SocketGuildUser guildUser)
            {
                var guildId = guildUser.Guild.Id;
                var collections = new List<IClaimCollection>();
                var claimCollection = (IClaimCollection?)await _claimManager.GetUserClaimCollectionAsync(guildId, guildUser.Id, default).ConfigureAwait(false);
                if (claimCollection != null)
                    collections.Add(claimCollection);
                foreach (var role in guildUser.Roles)
                {
                    claimCollection = await _claimManager.GetRoleClaimCollectionAsync(guildId, role.Id, default).ConfigureAwait(false);
                    if (claimCollection != null)
                        collections.Add(claimCollection);
                }

                return new CombinedClaimCollection(guildId, collections);
            }
            return new EmptyClaimCollection(0);
        }
    }
}
