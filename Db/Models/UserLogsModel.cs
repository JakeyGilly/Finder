namespace Finder.Bot.Db.Models;

public class UserLogsModel {
    public ulong GuildId { get; set; } // composite key
    public ulong UserId { get; set; } // composite key
    public int Bans { get; set; }
    public int Kicks { get; set; }
    public int Warns { get; set; }
    public int Mutes { get; set; }
    public DateTime? TempBan { get; set; }
    public DateTime? TempMute { get; set; }
}