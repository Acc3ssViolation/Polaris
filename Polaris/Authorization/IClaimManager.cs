using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    public interface IClaimManager
    {
        Task<IUserClaimCollection?> GetUserClaimCollectionAsync(ulong guildId, ulong userId, CancellationToken cancellationToken);
        Task<IRoleClaimCollection?> GetRoleClaimCollectionAsync(ulong guildId, ulong roleId, CancellationToken cancellationToken);

        Task<bool> UpdatePermissionClaimAsync(ulong guildId, ClaimType type, ulong subjectId, IPermissionClaim claim, CancellationToken cancellationToken);
        Task<bool> DeletePermissionClaimAsync(ulong guildId, ClaimType type, ulong subjectId, string claimIdentifier, CancellationToken cancellationToken);
    }
}
