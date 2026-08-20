using Finder.Bot.Db.Models;

namespace Finder.Bot.Db.Repositories;

public interface IUnitOfWork {
    IRepository<AddonsModel> Addons { get; }
    IRepository<TicketsModel> Ticketing { get; }
    IRepository<CountdownModel> Countdown { get; }
    IRepository<LevellingModel> Levelling { get; }
    IRepository<EconomyModel> Economy { get; }
    IRepository<InventoryModel> Inventory { get; }
    IRepository<PollsModel> Polls { get; }
    Task SaveChangesAsync();
}