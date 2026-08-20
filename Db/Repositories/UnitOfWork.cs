using Finder.Bot.Db.Models;

namespace Finder.Bot.Db.Repositories;

public class UnitOfWork(BotDbContext context) : IUnitOfWork {
    public IRepository<AddonsModel> Addons { get; } = new EfRepository<AddonsModel>(context);
    public IRepository<TicketsModel> Ticketing { get; } = new EfRepository<TicketsModel>(context);
    public IRepository<CountdownModel> Countdown { get; } = new EfRepository<CountdownModel>(context);
    public IRepository<LevellingModel> Levelling { get; } = new EfRepository<LevellingModel>(context);
    public IRepository<EconomyModel> Economy { get; } = new EfRepository<EconomyModel>(context);
    public IRepository<InventoryModel> Inventory { get; } = new EfRepository<InventoryModel>(context);
    public IRepository<PollsModel> Polls { get; } = new EfRepository<PollsModel>(context);
    
    public async Task SaveChangesAsync() {
        await context.SaveChangesAsync();
    }
}