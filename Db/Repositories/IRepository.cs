namespace Finder.Bot.Db.Repositories;

public interface IRepository<T> where T : class {
    Task<IEnumerable<T>> GetItemsAsync(string query);
    Task<IEnumerable<T>> GetAllItemsAsync();
    Task<T?> GetItemAsync(string id);
    Task AddItemAsync(T item);
    Task UpdateItemAsync(string? id, T item);
    Task UpsertItemAsync(string? id, T item);
    Task DeleteItemAsync(string id);
}