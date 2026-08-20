using System.Linq.Expressions;
namespace Finder.Web.Repositories;
public interface IRepository<T> where T : class {
    Task<T?> GetAsync(ulong id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> expression);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}