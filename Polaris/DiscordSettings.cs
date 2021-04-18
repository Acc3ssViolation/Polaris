using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris
{
    public class DiscordSettings
    {
        public string Token { get; set; } = string.Empty;
        public string CommandPrefix { get; set; } = "!";
        public string? StoragePath { get; set; }
    }
}
