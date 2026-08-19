using System.ComponentModel.DataAnnotations;
namespace Finder.Bot.Models.Data.Bot;

public class PollsModel {
    [Key]
    public Int64 MessageId { get; set; }
    public List<string> Answers { get; set; } = new();
    public List<Int64> VotersId { get; set; } = new();
}