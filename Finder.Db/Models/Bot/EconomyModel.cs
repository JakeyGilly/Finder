namespace Finder.Bot.Db.Models;

public class EconomyModel {
    public ulong GuildId { get; set; } // composite key
    public ulong UserId { get; set; } // composite key
    public int Money { get; set; }
    public int Bank { get; set; }
}
