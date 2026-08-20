namespace Finder.Bot.Db.Models;

public class InventoryModel {
    public ulong GuildId { get; set; } // composite key
    public ulong UserId { get; set; } // composite key
    public Guid ItemId { get; set; } // composite key
    public int Quantity { get; set; } = 1;
}