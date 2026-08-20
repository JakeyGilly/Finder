using System.Linq.Expressions;

namespace Finder.Bot.Db.Repositories;

public interface IRepository<T> where T : class {
    
    Task<List<T>> GetItemsAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    Task<T?> GetItemAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    
    void AddItem(T item);
    void DeleteItem(T item);
}