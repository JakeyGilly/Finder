using Finder.Bot.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace Finder.Bot.Db;

public class BotDbContext(DbContextOptions<BotDbContext> options) : DbContext(options) {
    public DbSet<AddonsModel> Addons => Set<AddonsModel>();
    public DbSet<TicketsModel> Tickets => Set<TicketsModel>();
    public DbSet<TicketUser> TicketUsers { get; set; }
    public DbSet<TicketClaimer> TicketClaimers { get; set; }
    public DbSet<LevellingModel> Leveling => Set<LevellingModel>();
    public DbSet<CountdownModel> Countdowns => Set<CountdownModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<LevellingModel>()
            .HasKey(x => new { x.GuildId, x.UserId });

        modelBuilder.Entity<AddonsModel>()
            .HasKey(x => new { x.GuildId, x.Addon });

        modelBuilder.Entity<TicketsModel>()
            .HasKey(x => x.ChannelId);

        // one-to-many relationship between TicketsModel and UsersModel
        modelBuilder.Entity<TicketsModel>()
            .HasMany(t => t.Users)
            .WithOne(u => u.Ticket)
            .HasForeignKey(u => u.TicketChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        // one-to-many relationship between TicketsModel and ClaimersModel
        modelBuilder.Entity<TicketsModel>()
            .HasMany(t => t.Claimers)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CountdownModel>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<EconomyModel>()
            .HasKey(x => new { x.GuildId, x.UserId });

        modelBuilder.Entity<InventoryModel>()
            .HasKey(x => new { x.GuildId, x.UserId, x.ItemId });
        
    }
}