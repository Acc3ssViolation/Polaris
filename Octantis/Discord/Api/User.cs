using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octantis.Discord.Api
{
    public class User
    {
        public ulong Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Discriminator { get; set; } = "0000";
    }
}
