using Polaris.Common;
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
        Task<IClaimCollection?> GetClaimCollectionAsync(GuildSubject subject, CancellationToken cancellationToken);

        Task SetPermissionClaimAsync(GuildSubject subject, string claim, CancellationToken cancellationToken);
        Task DeletePermissionClaimAsync(GuildSubject subject, string claim, CancellationToken cancellationToken);
    }
}
