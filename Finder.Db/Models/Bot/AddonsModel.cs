using Finder.Shared.Enum;

namespace Finder.Db.Models;

public class AddonsModel {
    public required ulong GuildId { get; set; } // guild Id
    public required Addons Addon { get; set; }
    public bool Enabled { get; set; }
}