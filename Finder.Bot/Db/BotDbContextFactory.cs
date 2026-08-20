using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Finder.Bot.Db;

public class BotDbContextFactory : IDesignTimeDbContextFactory<BotDbContext> {
    public BotDbContext CreateDbContext(string[] args) {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>() 
            .Build();

        var builder = new DbContextOptionsBuilder<BotDbContext>();
        var connectionString = configuration.GetConnectionString("PostgreSQL");
        builder.UseNpgsql(connectionString);
        return new BotDbContext(builder.Options);
    }
}