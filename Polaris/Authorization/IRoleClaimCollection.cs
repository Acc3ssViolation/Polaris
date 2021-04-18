﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.Authorization
{
    public interface IRoleClaimCollection : IClaimCollection
    {
        public ulong RoleId { get; }
    }
}
