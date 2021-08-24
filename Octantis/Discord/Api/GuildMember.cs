using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Octantis.Discord.Api
{
    public class GuildMember
    {
        public User? User { get; set; }
        [JsonPropertyName("nick")]
        public string? Nickname { get; set; }
        [JsonPropertyName("joined_at")]
        public DateTimeOffset JoinedAt { get; set; }
    }
}
