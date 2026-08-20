using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Finder.Db;

public class FinderDbContextFactory : IDesignTimeDbContextFactory<FinderDbContext> {
    public FinderDbContext CreateDbContext(string[] args) {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<FinderDbContextFactory>() 
            .Build();

        var builder = new DbContextOptionsBuilder<FinderDbContext>();
        var connectionString = configuration.GetConnectionString("PostgreSQL");
        builder.UseNpgsql(connectionString);
        return new FinderDbContext(builder.Options);
    }
}