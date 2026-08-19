namespace Finder.Bot.Db.Models;

public class SettingsModel {
    public string GuildId { get; set; } // pk
    public Dictionary<string, string> Settings { get; set; }
}