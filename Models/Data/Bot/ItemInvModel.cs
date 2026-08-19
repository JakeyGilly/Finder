using System.ComponentModel.DataAnnotations;
namespace Finder.Bot.Models.Data.Bot;

public class ItemInvModel {
    [Key]
    public Int64 GuildId { get; set; }
    [Key]
    public Int64 UserId { get; set; }
    public List<Guid> ItemIds { get; set; } = new();

}