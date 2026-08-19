namespace Finder.Bot.Db.Models;

public class PollsModel {
    public ulong MessageId { get; set; } // pk
    public List<string> Answers { get; set; } = new();
    public List<ulong> VotersId { get; set; } = new();
}