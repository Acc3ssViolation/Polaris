using Discord.WebSocket;

namespace Polaris.Common
{
    public enum SubjectType
    {
        User,
        Role,
        Channel,
        Emoji
    }

    public record GuildSubject(SubjectType Type, ulong Id, ulong GuildId)
    {
        public static GuildSubject FromGuildUser(SocketGuildUser user) => new GuildSubject(SubjectType.User, user.Id, user.Guild.Id);
        public static GuildSubject FromRole(SocketRole role) => new GuildSubject(SubjectType.Role, role.Id, role.Guild.Id);
    }
}
