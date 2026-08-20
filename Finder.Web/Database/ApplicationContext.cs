using Finder.Web.Models.Data.Bot;
using Finder.Web.Models.Data.Web;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
namespace Finder.Web.Database;

public class ApplicationContext : DbContext {
    public ApplicationContext(DbContextOptions options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder builder) {
        builder.Entity<AddonsModel>().Property(x => x.Addons).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v) ?? new Dictionary<string, string>());
        builder.Entity<UserSettingsModel>().Property(x => x.Settings).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v) ?? new Dictionary<string, string>());
    }
    
    // Finder.Bot
    public DbSet<AddonsModel> Addons { get; set; }

    // Finder.Web
    public DbSet<UserSettingsModel> UserSettings { get; set; }
}
