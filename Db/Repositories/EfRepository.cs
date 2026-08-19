using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Finder.Bot.Db.Repositories;

public class EfRepository<T>(BotDbContext context) : IRepository<T> where T : class {
    protected readonly BotDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<List<T>> GetItemsAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();
    
    public async Task<T?> GetItemAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.FirstOrDefaultAsync(predicate);

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