using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    internal class UserClaimCollection : IUserClaimCollection
    {
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }

        public IReadOnlyCollection<IPermissionClaim> Claims { get; set; }

        public UserClaimCollection(ulong guildId, ulong userId, IEnumerable<IPermissionClaim> claims)
        {
            GuildId = guildId;
            UserId = userId;
            Claims = new List<IPermissionClaim>(claims);
        }
    }
}
