using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    public static class ClaimExtensions
    {
        public static bool HasClaim(this IClaimCollection collection, string identifier, Operation operation)
        {
            return collection.Claims.Any(c => c.Identifier == identifier && (c.ClaimedOperations & operation) == operation);
        }
    }
}
