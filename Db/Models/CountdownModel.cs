namespace Finder.Bot.Db.Models;

public class CountdownModel {
    public required string Id { get; set; } // pk
    public required ulong ChannelId { get; set; }
    public required ulong GuildId { get; set; }
    public required long UnixTime { get; set; }
    public ulong? PingUserId { get; set; }
    public ulong? PingRoleId { get; set; }
}