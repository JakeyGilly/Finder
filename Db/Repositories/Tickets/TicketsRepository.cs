namespace Finder.Bot.Db.Repositories.Tickets;

public class TicketsRepository(ICosmosDbService cosmosDbService) : ITicketsRepository
{
    private readonly ICosmosDbService _cosmosDbService = cosmosDbService;

    // public async Task<bool> AddonEnabled(ulong guildId, string addonName) {
    //     var addonData = await _cosmosDbService.GetItemAsync(guildId.ToString());
    //     
    //     return addonData != null && 
    //            addonData.Addons.TryGetValue(addonName, out bool enabled) && 
    //            enabled;
    // }
}