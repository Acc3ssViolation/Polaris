using System.Text.Json.Serialization;

namespace Octantis.Discord.Api
{
    public enum Opcode
    {
        Heartbeat = 1,
        Identify = 2,
        Hello = 10,
    }

    public class GatewayPacket<T>
    {
        [JsonPropertyName("op")]
        public Opcode Opcode { get; set; }
        [JsonPropertyName("d")]
        public T? Data { get; set; }
        [JsonPropertyName("s")]
        public int? SequenceNumber { get; set; }
        [JsonPropertyName("e")]
        public string? EventName { get; set; }
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