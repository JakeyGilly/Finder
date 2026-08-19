using System.ComponentModel.DataAnnotations;

namespace Finder.Bot.Db.Models;

public class TicketsModel {
    [Key]
    public required ulong ChannelId { get; set; } // pk
    public required ulong GuildId { get; set; }
    public required ulong IntroMessageId { get; set; }
    public List<ulong> UserIds { get; set; } = new();
    public string? Name { get; set; }
    public List<ulong> ClaimedUserId { get; set; } = new();
}