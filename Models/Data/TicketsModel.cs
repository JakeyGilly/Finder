using System.ComponentModel.DataAnnotations;
using Finder.Bot.Db;

namespace Finder.Bot.Models.Data;

public class TicketsModel: ICosmosItem {
    [Key]
    public required string Id { get; set; } // ticket channel Id
    public required ulong GuildId { get; set; }
    public required ulong IntroMessageId { get; set; }
    public List<ulong> UserIds { get; set; } = new();
    public string? Name { get; set; }
    public List<ulong> ClaimedUserId { get; set; } = new();
}