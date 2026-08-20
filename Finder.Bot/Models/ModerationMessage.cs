using Finder.Bot.Enums;

namespace Finder.Bot.Models;

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