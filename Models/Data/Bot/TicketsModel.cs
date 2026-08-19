using System.ComponentModel.DataAnnotations;
namespace Finder.Bot.Models.Data.Bot;

public class TicketsModel {
    [Key]
    public Int64 SupportChannelId { get; set; }
    public Int64 GuildId { get; set; }
    public Int64? IntroMessageId { get; set; }
    public List<Int64?> UserIds { get; set; } = new();
    public string? Name { get; set; }
    public List<Int64> ClaimedUserId { get; set; } = new();
}