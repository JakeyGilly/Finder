namespace Finder.Db.Models.Web;

public class UserSettingsModel {
    public ulong UserId { get; set; } // composite key
    public string Setting { get; set; } // composite key
    public string Value { get; set; }
}