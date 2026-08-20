using Finder.Web.Repositories.Bot;
using Finder.Web.Repositories.Web;
namespace Finder.Web.Repositories;

public interface IUnitOfWork {
    IAddonsRepository Addons { get; }
    IUserSettingsRepository UserSettings { get; }
    Task<int> SaveChangesAsync();
    Task DisposeAsync();
}