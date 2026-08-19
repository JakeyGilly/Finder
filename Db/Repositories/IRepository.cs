using System.Linq.Expressions;

namespace Finder.Bot.Db.Repositories;

public interface IRepository<T> where T : class {
    Task<List<T>> GetItemsAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetItemAsync(Expression<Func<T, bool>> predicate);
    Task AddItemAsync(T item);
    Task UpdateItemAsync(Expression<Func<T, bool>> predicate, T item);
    Task UpsertItemAsync(Expression<Func<T, bool>> predicate, T item);
    Task DeleteItemAsync(Expression<Func<T, bool>> predicate);
}