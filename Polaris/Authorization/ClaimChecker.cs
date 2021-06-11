using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    public static class ClaimChecker
    {
        public static bool IsAllowed(IClaimCollection claims, string requiredPermission, bool inheritParentPermission)
        {
            var permissionName = requiredPermission;

            do
            {
                var claim = claims.Claims.FirstOrDefault(c => c.Identifier == permissionName);
                if (claim is not null)
                    return claim.Allow;

                var parentNameEndIndex = permissionName.IndexOf('.');
                if (parentNameEndIndex == -1)
                    break;

                permissionName = permissionName.Substring(0, parentNameEndIndex);
            } while (inheritParentPermission);

            return false;
        }
    }
}
