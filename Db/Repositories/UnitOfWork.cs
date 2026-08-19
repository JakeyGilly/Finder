using Finder.Bot.Db.Repositories.Addons;

namespace Finder.Bot.Db.Repositories;

public class UnitOfWork : IUnitOfWork {
    public IAddonsRepository Addons { get; }

    public UnitOfWork(ICosmosDbService cosmosDbService) {
        Addons = new AddonsRepository(cosmosDbService);
    }
}