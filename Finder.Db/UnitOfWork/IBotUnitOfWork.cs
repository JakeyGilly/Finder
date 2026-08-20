using Finder.Bot.Db.Models;
using Finder.Db.Models;
using Finder.Db.Repositories;

namespace Finder.Db.UnitOfWork;

public interface IBotUnitOfWork {
    IRepository<AddonsModel> Addons { get; }
    IRepository<TicketsModel> Ticketing { get; }
    IRepository<CountdownModel> Countdown { get; }
    IRepository<LevellingModel> Levelling { get; }
    IRepository<EconomyModel> Economy { get; }
    IRepository<InventoryModel> Inventory { get; }
    IRepository<PollsModel> Polls { get; }
    IRepository<UserLogsModel> UserLogs { get; }
    IRepository<SettingsModel> Settings { get; }
    Task SaveChangesAsync();
}