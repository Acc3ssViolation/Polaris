using System;
using System.Collections.Generic;

namespace Octantis.Discord.Api
{
    public class Guild
    {
        public ulong Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public IReadOnlyList<Channel> Channels { get; set; } = Array.Empty<Channel>();
    }
}