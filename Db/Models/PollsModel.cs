using System.ComponentModel.DataAnnotations;

namespace Finder.Bot.Db.Models;

public class PollsModel {
    [Key]
    public Int64 Id { get; set; } // message Id
    public List<string> Answers { get; set; } = new();
    public List<Int64> VotersId { get; set; } = new();
}