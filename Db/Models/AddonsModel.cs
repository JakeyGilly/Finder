using Finder.Bot.Enums;

namespace Finder.Bot.Db.Models;

public class AddonsModel {
    public required ulong GuildId { get; set; } // guild Id
    public required Addons Addon { get; set; }
    public required bool Enabled { get; set; }
}