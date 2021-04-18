using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    internal class ServerPermissionManager : IPermissionManager
    {
        // TOOD: Store in database...
        private ConcurrentDictionary<string, IPermission> _permissions = new ConcurrentDictionary<string, IPermission>();

        public Task<bool> CreatePermissionAsync(IPermission permission, CancellationToken cancellationToken)
        {
            return Task.FromResult(_permissions.TryAdd(permission.Identifier, permission));
        }

        public Task<bool> DeletePermissionAsync(string identifier, CancellationToken cancellationToken)
        {
            return Task.FromResult(_permissions.TryRemove(identifier, out var permission));
        }

        public Task<IPermission?> GetPermissionAsync(string identifier, CancellationToken cancellationToken)
        {
            _permissions.TryGetValue(identifier, out var permission);
            return Task.FromResult(permission);
        }

        public Task<IReadOnlyList<IPermission>> GetPermissionsAsync(string identifier, Func<IPermission, bool> predicate, CancellationToken cancellationToken)
        {
            var values = _permissions.Values.ToList();
            return Task.FromResult<IReadOnlyList<IPermission>>(values.Where(predicate).ToList());
        }

        public Task<bool> UpdatePermissionAsync(IPermission permission, CancellationToken cancellationToken)
        {
            if (_permissions.ContainsKey(permission.Identifier))
            {
                _permissions.AddOrUpdate(permission.Identifier, permission, (_, _) => permission);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
