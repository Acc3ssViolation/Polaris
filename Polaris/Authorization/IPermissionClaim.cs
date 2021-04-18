using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    public interface IPermissionClaim
    {
        public string Identifier { get; }
        public Operation ClaimedOperations { get; }
    }
}
