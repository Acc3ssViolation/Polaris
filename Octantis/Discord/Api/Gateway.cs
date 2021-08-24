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

    public enum Event
    {
        Unknown,
        Ready,
        GuildCreate,
        InteractionCreate,
    }

    public static class Events
    {
        public const string Ready = "READY";

        private static readonly Dictionary<string, Event> EventMap = new Dictionary<string, Event>
        {
            { "GUILD_CREATE", Event.GuildCreate },
            { "INTERACTION_CREATE", Event.InteractionCreate },
            { "READY", Event.Ready },
        };

        public static Event FromString(string eventType)
        {
            if (EventMap.TryGetValue(eventType, out var result))
                return result;
            return Event.Unknown;
        }
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
        [JsonPropertyName("id")]
        public ulong Id { get; set; }
    }

    public class ReadyData
    {
        [JsonPropertyName("v")]
        public int GatewayVersion { get; set; }
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = string.Empty;
        [JsonPropertyName("guilds")]
        public IReadOnlyList<UnavailableGuild> Guilds { get; set; } = Array.Empty<UnavailableGuild>();
        public User? User { get; set; }
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