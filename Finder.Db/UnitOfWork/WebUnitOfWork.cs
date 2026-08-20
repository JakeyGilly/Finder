using Finder.Bot.Db.Models;
using Finder.Db.Models;
using Finder.Db.Models.Web;
using Finder.Db.Repositories;

namespace Finder.Db.UnitOfWork;

public class WebUnitOfWork(FinderDbContext context) : IWebUnitOfWork {
    public IRepository<AddonsModel> Addons { get; }
    public IRepository<TicketsModel> Ticketing { get; }
    public IRepository<CountdownModel> Countdown { get; }
    public IRepository<LevellingModel> Levelling { get; }
    public IRepository<EconomyModel> Economy { get; }
    public IRepository<InventoryModel> Inventory { get; }
    public IRepository<PollsModel> Polls { get; }
    public IRepository<UserLogsModel> UserLogs { get; }
    public IRepository<SettingsModel> Settings { get; }
    

    public IRepository<UserSettingsModel> UserSettings { get; }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}