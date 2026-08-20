namespace Finder.Bot.Db.Models;

public class PollsModel {
    public ulong MessageId { get; set; } // pk
    
    // Navigation properties for related entities (EF core)
    public ICollection<PollAnswer> Answers { get; set; } = [];
    public ICollection<PollVoter> Voters { get; set; } = [];
}

public class PollAnswer {
    public int Id { get; set; } // pk
    public required string Answer { get; set; }
    
    // Foreign Key mapping back to the Poll
    public required ulong PollMessageId { get; set; } 
    public PollsModel Poll { get; set; } = null!; // Navigation property
}

public class PollVoter {
    public int Id { get; set; } // pk
    public required ulong UserId { get; set; }
    
    // Foreign Key mapping back to the Poll
    public required ulong PollMessageId { get; set; } 
    public PollsModel Poll { get; set; } = null!; // Navigation property
}