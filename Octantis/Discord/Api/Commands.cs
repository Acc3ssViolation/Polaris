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
        public ulong? ApplicationId { get; set; }
        [JsonPropertyName("guild_id")]
        public ulong? GuildId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("default_permission")]
        public bool? DefaultPremission { get; set; } = true;
        public IList<ApplicationCommandOption>? Options { get; set; }
    }

    public enum ApplicationCommandOptionType
    {
        SubCommand = 1,
        SubCommandGroup = 2,
        String = 3,
        Integer = 4,
        Boolean = 5,
        User = 6,
        Channel = 7,
        Role = 8,
        Mentionable = 9,
        Number = 10,
    }

    public class ApplicationCommandOption
    {
        public ApplicationCommandOptionType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool? Required { get; set; }
        public IList<ApplicationCommandOption>? Options { get; set; }
    }
}
