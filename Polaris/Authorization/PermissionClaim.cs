using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    internal class PermissionClaim : IPermissionClaim
    {
        public string Identifier { get; set; }

        public Operation ClaimedOperations { get; set; }

        public PermissionClaim(string identifier, Operation claimedOperations)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            ClaimedOperations = claimedOperations;
        }
    }
}
