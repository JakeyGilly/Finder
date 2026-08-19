using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Finder.Bot.Db;

public class BotDbContextFactory : IDesignTimeDbContextFactory<BotDbContext> {
    public BotDbContext CreateDbContext(string[] args) {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var builder = new DbContextOptionsBuilder<BotDbContext>();
        var connectionString = configuration.GetConnectionString("PostgreSQL");
        builder.UseNpgsql(connectionString);
        return new BotDbContext(builder.Options);
    }
}