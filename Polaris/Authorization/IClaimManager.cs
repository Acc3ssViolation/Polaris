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
        Task<IUserClaimCollection?> GetUserClaimCollectionAsync(ulong userId, CancellationToken cancellationToken);
        Task<IRoleClaimCollection?> GetRoleClaimCollectionAsync(ulong roleId, CancellationToken cancellationToken);

        Task<bool> CreateClaimCollectionAsync(IClaimCollection collection, CancellationToken cancellationToken);
        Task<bool> UpdateClaimCollectionAsync(IClaimCollection collection, CancellationToken cancellationToken);
        Task<bool> DeleteClaimCollectionAsync(IClaimCollection collection, CancellationToken cancellationToken);
    }
}
