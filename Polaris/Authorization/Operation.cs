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
        Get = 1,
        Set = 2,
        Delete = 4
    }
}
