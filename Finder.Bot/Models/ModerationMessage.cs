using Finder.Bot.Modules.Helpers.Enums;
namespace Finder.Bot.Modules.Helpers;

public class ModerationMessage {
    public ulong MessageId { get; set; }
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong SenderId { get; set; }
    public ulong UserId { get; set; }
    public string Reason { get; set; } = "No reason given.";
    public DateTime? Time { get; set; } = null;
    public ModerationMessageType Type { get; set; }
}