using Finder.Bot.Db.Models;
using Microsoft.EntityFrameworkCore;

namespace Finder.Bot.Db;

public class BotDbContext(DbContextOptions<BotDbContext> options) : DbContext(options) {
    public DbSet<AddonsModel> Addons => Set<AddonsModel>();
    public DbSet<TicketsModel> Tickets => Set<TicketsModel>();
    public DbSet<LevellingModel> Leveling => Set<LevellingModel>();
    public DbSet<CountdownModel> Countdowns => Set<CountdownModel>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<LevellingModel>()
            .HasKey(x => new { x.GuildId, x.UserId });

        modelBuilder.Entity<AddonsModel>()
            .HasKey(x => new { x.GuildId, x.Addon });
        
        modelBuilder.Entity<TicketsModel>()
            .HasKey(x => x.ChannelId);
        
        modelBuilder.Entity<CountdownModel>()
            .HasKey(x => x.Id);
        
        modelBuilder.Entity<EconomyModel>()
            .HasKey(x => new { x.GuildId, x.UserId });
        
        modelBuilder.Entity<InventoryModel>()
            .HasKey(x => new { x.GuildId, x.UserId, x.ItemId });
        
        // change this
        modelBuilder.Entity<TicketsModel>()
            .Property(x => x.UserIds)
            .HasColumnType("jsonb");

        modelBuilder.Entity<TicketsModel>()
            .Property(x => x.ClaimedUserId)
            .HasColumnType("jsonb");
    }
}