using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    [Flags]
    public enum Operation
    {
        None = 0,
        Create = 1,
        Read = 2,
        Update = 4,
        Delete = 8
    }
}
