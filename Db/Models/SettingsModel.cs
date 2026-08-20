namespace Finder.Bot.Db.Models;

public class SettingsModel {
    public ulong GuildId { get; set; } // composite key
    public string Setting { get; set; } // composite key
    public string Value { get; set; }
}