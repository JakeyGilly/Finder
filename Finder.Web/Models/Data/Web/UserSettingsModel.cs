using System.ComponentModel.DataAnnotations;
namespace Finder.Web.Models.Data.Web;

public class UserSettingsModel {
    [Key]
    public Int64 UserId { get; set; }
    public Dictionary<string, string> Settings { get; set; }
}