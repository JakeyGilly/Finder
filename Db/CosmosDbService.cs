using Finder.Bot.Models.Data.Bot;

namespace Finder.Bot.Db;

using Microsoft.Azure.Cosmos;

public class CosmosDbService : ICosmosDbService {
    private Container _container;
    public CosmosDbService(CosmosClient dbClient, string databaseName, string containerName) {
        _container = dbClient.GetContainer(databaseName, containerName);
    }
    public async Task AddItemAsync(AddonsModel item) {
        await _container.CreateItemAsync(item, new PartitionKey(item.Id));
    }
    public async Task DeleteItemAsync(string id) {
        await _container.DeleteItemAsync<AddonsModel>(id, new PartitionKey(id));
    }
    public async Task<AddonsModel?> GetItemAsync(string id) {
        try {
            ItemResponse<AddonsModel> response = await _container.ReadItemAsync<AddonsModel>(id, new PartitionKey(id));
            return response.Resource;
        } catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) { 
            return null;
        }
    }
    public async Task<IEnumerable<AddonsModel>> GetItemsAsync(string queryString) {
        var query = _container.GetItemQueryIterator<AddonsModel>(new QueryDefinition(queryString));
        List<AddonsModel> results = new List<AddonsModel>();
        while (query.HasMoreResults) {
            var response = await query.ReadNextAsync();
            
            results.AddRange(response.ToList());
        }
        return results;
    }
    public async Task UpdateItemAsync(string? id, AddonsModel item) {
        await _container.UpsertItemAsync<AddonsModel>(item, new PartitionKey(id));
    }
}
