using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    internal class RoleClaimCollection : IRoleClaimCollection
    {
        public ulong GuildId { get; set; }

        public ulong RoleId { get; set; }

        public IReadOnlyCollection<IPermissionClaim> Claims { get; set; }

        public RoleClaimCollection(ulong guildId, ulong roleId, IEnumerable<IPermissionClaim> claims)
        {
            GuildId = guildId;
            RoleId = roleId;
            Claims = new List<IPermissionClaim>(claims);
        }
    }
}
