using Finder.Bot.Db.Models;
using Finder.Db.Models;
using Finder.Db.Models.Web;
using Finder.Db.Repositories;

namespace Finder.Db.UnitOfWork;

public class WebUnitOfWork(FinderDbContext context) : IWebUnitOfWork {
    public IRepository<AddonsModel> Addons { get; } = new EfRepository<AddonsModel>(context);
    public IRepository<TicketsModel> Ticketing { get; } = new EfRepository<TicketsModel>(context);
    public IRepository<CountdownModel> Countdown { get; } = new EfRepository<CountdownModel>(context);
    public IRepository<LevellingModel> Levelling { get; } = new EfRepository<LevellingModel>(context);
    public IRepository<EconomyModel> Economy { get; } = new EfRepository<EconomyModel>(context);
    public IRepository<InventoryModel> Inventory { get; } = new EfRepository<InventoryModel>(context);
    public IRepository<PollsModel> Polls { get; } = new EfRepository<PollsModel>(context);
    public IRepository<UserLogsModel> UserLogs { get; } = new EfRepository<UserLogsModel>(context);
    public IRepository<SettingsModel> Settings { get; } = new EfRepository<SettingsModel>(context);
    

    public IRepository<UserSettingsModel> UserSettings { get; } = new EfRepository<UserSettingsModel>(context);

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}