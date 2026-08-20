namespace Finder.Bot.Db.Models;

public class TicketsModel {
    public required ulong ChannelId { get; set; } // pk
    public required ulong GuildId { get; set; }
    public required ulong IntroMessageId { get; set; }
    public string? Name { get; set; }
    
    // Navigation properties for related entities (EF core)
    public ICollection<TicketUser> Users { get; set; } = [];
    public ICollection<TicketClaimer> Claimers { get; set; } = [];
}

public class TicketUser {
    public int Id { get; set; } // pk
    public required ulong UserId { get; set; }
    
    // Foreign Key mapping back to the Ticket
    public required ulong TicketChannelId { get; set; } 
    public TicketsModel Ticket { get; set; } = null!; // Navigation property
}

public class TicketClaimer {
    public int Id { get; set; } // pk
    public required ulong UserId { get; set; }
    
    // Foreign Key mapping back to the Ticket
    public required ulong TicketChannelId { get; set; } 
    public TicketsModel Ticket { get; set; } = null!; // Navigation property
}