using Finder.Bot.Db.Exceptions;
using Finder.Bot.Models.Data;
using Microsoft.Azure.Cosmos;

namespace Finder.Bot.Db.Repositories.Tickets;

public class TicketsRepository(CosmosClient dbClient, string databaseName) : CosmosRepository<TicketsModel>(dbClient, databaseName, "tickets"), ITicketsRepository {
    public async Task<TicketsModel> GetTicketAsync(ulong channelId) {
        return await GetItemAsync(channelId.ToString()) ?? throw new EntityNotFoundException<TicketsModel>(channelId);
    }
}