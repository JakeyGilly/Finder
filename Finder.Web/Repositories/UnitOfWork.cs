using Finder.Web.Database;
using Finder.Web.Repositories.Bot;
using Finder.Web.Repositories.Web;
namespace Finder.Web.Repositories;

public class UnitOfWork : IUnitOfWork{
    private readonly ApplicationContext _context;
    public IAddonsRepository Addons { get; }
    public IUserSettingsRepository UserSettings { get; }
    public UnitOfWork(ApplicationContext context) {
        _context = context;
        Addons = new AddonsRepository(_context);
        UserSettings = new UserSettingsRepository(_context);
    }
    public async Task<int> SaveChangesAsync() {
        return await _context.SaveChangesAsync();
    }
    public async Task DisposeAsync() {
        await _context.DisposeAsync();
    }
}