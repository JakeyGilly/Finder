using Finder.Bot.Db;

namespace Finder.Bot.Models.Data;

public class CountdownModel: ICosmosItem {
    public required string Id { get; set; } // Guid
    public required ulong ChannelId { get; set; }
    public required ulong GuildId { get; set; }
    public required long UnixTime { get; set; }
    public ulong? PingUserId { get; set; }
    public ulong? PingRoleId { get; set; }
}