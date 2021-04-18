using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    public interface IPermissionManager
    {
        Task<IPermission?> GetPermissionAsync(string identifier, CancellationToken cancellationToken);
        Task<IReadOnlyList<IPermission>> GetPermissionsAsync(string identifier, Func<IPermission, bool> predicate, CancellationToken cancellationToken);

        Task<bool> CreatePermissionAsync(IPermission permission, CancellationToken cancellationToken);
        Task<bool> UpdatePermissionAsync(IPermission permission, CancellationToken cancellationToken);
        Task<bool> DeletePermissionAsync(string identifier, CancellationToken cancellationToken);
    }
}
