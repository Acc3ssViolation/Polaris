using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octantis
{
    public class DiscordSettings
    {
        public string ModRole { get; set; } = string.Empty;
        public ulong TestGuildId { get; set; } = 0;
        public string Token { get; set; } = string.Empty;
    }
}
