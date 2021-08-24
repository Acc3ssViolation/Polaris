using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Octantis.Discord.Api
{
    public enum ApplicationCommandType
    {
        ChatInput = 1,
        User = 2,
        Message = 3,
    }

    public class ApplicationCommand
    {
        public ulong Id { get; set; }
        public ApplicationCommandType? Type { get; set; }
        [JsonPropertyName("application_id")]
        public ulong ApplicationId { get; set; }
        [JsonPropertyName("guild_id")]
        public ulong? GuildId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("default_permission")]
        public bool? DefaultPremission { get; set; } = true;
    }
}
