using Finder.Bot.Db.Repositories.Addons;

namespace Finder.Bot.Db.Repositories;

public interface IUnitOfWork {
    IAddonsRepository Addons { get; }
    
}