using Polaris.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    public interface IClaimCollection
    {
        public GuildSubject Subject { get; }
        public IReadOnlyList<Claim> Claims { get; }
    }
}
