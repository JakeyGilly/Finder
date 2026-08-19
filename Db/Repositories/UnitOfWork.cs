using Finder.Bot.Db.Repositories.Addons;
using Finder.Bot.Db.Repositories.Levelling;
using Finder.Bot.Db.Repositories.Tickets;
using Finder.Bot.Models.Data;
using Microsoft.Azure.Cosmos;

namespace Finder.Bot.Db.Repositories;

public class UnitOfWork(CosmosClient dbClient, string databaseName) : IUnitOfWork {
    public IAddonsRepository Addons { get; } = new AddonsRepository(dbClient, databaseName);
    public ITicketsRepository Ticketing { get; } = new TicketsRepository(dbClient, databaseName);
    public IRepository<CountdownModel> Countdown { get; } = new CosmosRepository<CountdownModel>(dbClient, databaseName, "countdown");
    public ILevellingRepository Levelling { get; } = new LevellingRepository(dbClient, databaseName);
}