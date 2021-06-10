using Polaris.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    public record ClaimCollection(GuildSubject Subject, IReadOnlyList<string> Claims): IClaimCollection;
}
