using System.ComponentModel.DataAnnotations;

namespace Finder.Bot.Db.Models;

public class UserLogsModel {
    [Key]
    public Int64 GuildId { get; set; }
    [Key]
    public Int64 UserId { get; set; }
    public int Bans { get; set; }
    public int Kicks { get; set; }
    public int Warns { get; set; }
    public int Mutes { get; set; }
    public DateTime? TempBan { get; set; }
    public DateTime? TempMute { get; set; }
}