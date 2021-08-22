using System.Text.Json.Serialization;

namespace Octantis.Discord.Api
{
    public enum Opcode
    {
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

    public record GetGatewayResponse(string Url);
}