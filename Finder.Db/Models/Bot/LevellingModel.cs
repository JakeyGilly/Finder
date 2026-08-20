namespace Finder.Bot.Db.Models;

public class LevellingModel {
    public ulong GuildId { get; set; } // composite key
    public ulong UserId { get; set; } // composite key
    public int Level { get; set; }
    public int Exp { get; set; }
}