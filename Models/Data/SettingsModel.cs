using System.ComponentModel.DataAnnotations;
namespace Finder.Bot.Models.Data;

public class SettingsModel {
    [Key]
    public string Id { get; set; } // guild Id
    public Dictionary<string, string> Settings { get; set; }
}