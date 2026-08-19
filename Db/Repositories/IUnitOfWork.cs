using Finder.Bot.Db.Repositories.Addons;
using Finder.Bot.Db.Repositories.Levelling;
using Finder.Bot.Db.Repositories.Tickets;
using Finder.Bot.Models.Data;

namespace Finder.Bot.Db.Repositories;

public interface IUnitOfWork {
    IAddonsRepository Addons { get; }
    ITicketsRepository Ticketing { get; }
    IRepository<CountdownModel> Countdown { get; }
    ILevellingRepository Levelling { get; }
}