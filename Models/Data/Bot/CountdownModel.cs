namespace Finder.Bot.Models.Data.Bot;

public class CountdownModel {
    public int Id { get; set; }
    public Int64 ChannelId { get; set; }
    public Int64 GuildId { get; set; }
    public long UnixTime { get; set; }
    public Int64? PingUserId { get; set; }
    public Int64? PingRoleId { get; set; }
}