using Finder.Bot.Db.Models;
using Finder.Bot.Db.Repositories.Addons;

namespace Finder.Bot.Db.Repositories;

public class UnitOfWork(BotDbContext context) : IUnitOfWork {
    public IAddonsRepository Addons { get; } = new AddonsRepository(context);
    public IRepository<TicketsModel> Ticketing { get; } = new EfRepository<TicketsModel>(context);
    public IRepository<CountdownModel> Countdown { get; } = new EfRepository<CountdownModel>(context);
    public IRepository<LevellingModel> Levelling { get; } = new EfRepository<LevellingModel>(context);
}