namespace Octantis.Discord.Api
{
    public enum ChannelType
    {
        GuildText = 0,
        DirectMessage = 1,
        GuildVoice = 2,
        GroupDirectMessage = 3,
        GuildCategory = 4,
        GuildNews = 5,
        GuildStore = 6,
        GuildNewsThread = 10,
        GuildPublicThread = 11,
        GuildStageVoice = 13,
    }

    public class Channel
    {
        public ulong Id { get; set; }
        public ChannelType Type { get; set; }
        public ulong? GuildId { get; set; }
    }
}