using Finder.Bot.Models.Data.Bot;

namespace Finder.Bot.Db.Repositories.Tickets;

public interface ITicketsRepository {
    Task<TicketsModel> GetAsync(ulong guildId, string ticketId);
    Task<List<TicketsModel>> GetAllAsync(ulong guildId);
}