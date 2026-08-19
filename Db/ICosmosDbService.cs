using Finder.Bot.Models.Data.Bot;

namespace Finder.Bot.Db;

public interface ICosmosDbService {
    Task<IEnumerable<AddonsModel>> GetItemsAsync(string query);
    Task<AddonsModel?> GetItemAsync(string id);
    Task AddItemAsync(AddonsModel item);
    Task UpdateItemAsync(string? id, AddonsModel item);
    Task DeleteItemAsync(string id);
}
