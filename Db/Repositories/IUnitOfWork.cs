using Finder.Bot.Db.Models;
using Finder.Bot.Db.Repositories.Addons;

namespace Finder.Bot.Db.Repositories;

public interface IUnitOfWork {
    IAddonsRepository Addons { get; }
    IRepository<TicketsModel> Ticketing { get; }
    IRepository<CountdownModel> Countdown { get; }
    IRepository<LevellingModel> Levelling { get; }
}