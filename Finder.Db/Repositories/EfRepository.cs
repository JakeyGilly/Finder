using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Finder.Db.Repositories;

public class EfRepository<T>(FinderDbContext context) : IRepository<T> where T : class {
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<List<T>> GetItemsAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes) {
        IQueryable<T> query = _dbSet;
        foreach (var include in includes) {
            query = query.Include(include);
        }
        return await query.Where(predicate).ToListAsync();
    }
    
    public async Task<T?> GetItemAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes) {
        IQueryable<T> query = _dbSet;
        foreach (var include in includes) {
            query = query.Include(include);
        }
        return await query.FirstOrDefaultAsync(predicate);
    }

    // these dont do db calls yet.
    // mark for addition
    public void AddItem(T item) {
        _dbSet.Add(item);
    }

    // mark for deletion
    public void DeleteItem(T item) {
        _dbSet.Remove(item);
    }
}