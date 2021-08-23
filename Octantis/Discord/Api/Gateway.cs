using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Octantis.Discord.Api
{
    public enum Opcode
    {
        Dispatch = 0,
        Heartbeat = 1,
        Identify = 2,
        Hello = 10,
        HeartbeatAck = 11,
    }

    public static class Events
    {
        public const string Ready = "READY";
        public const string GuildCreate = "GUILD_CREATE";
    }

    public class GatewayPacket<T>
    {
        [JsonPropertyName("op")]
        public Opcode Opcode { get; set; }
        [JsonPropertyName("d")]
        public T? Data { get; set; }
        [JsonPropertyName("s")]
        public int? SequenceNumber { get; set; }
        [JsonPropertyName("t")]
        public string? EventName { get; set; }
    }

    public class UnavailableGuild
    {
        // TODO: Proper support for 64 bit snowflakes
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    public class ReadyData
    {
        [JsonPropertyName("v")]
        public int GatewayVersion { get; set; }
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = string.Empty;
        [JsonPropertyName("guilds")]
        public IReadOnlyList<UnavailableGuild> Guilds { get; set; } = Array.Empty<UnavailableGuild>();
    }

    public class HelloData
    {
        [JsonPropertyName("heartbeat_interval")]
        public int HeartbeatInterval { get; set; }
    }

    public class IdentifyData
    {
        public string Token { get; set; } = string.Empty;
        public int Intents { get; set; }
        public IdentifyDataProperties Properties { get; set; } = new IdentifyDataProperties();
    }

    public class IdentifyDataProperties
    {
        public string Os { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
    }

    public record GetGatewayResponse(string Url);
}