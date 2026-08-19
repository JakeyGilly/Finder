using Finder.Bot.Models.Data;

namespace Finder.Bot.Db.Repositories.Tickets;

public interface ITicketsRepository: IRepository<TicketsModel> {
    Task<TicketsModel> GetTicketAsync(ulong channelId);
}