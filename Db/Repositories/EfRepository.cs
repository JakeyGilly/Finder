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

    public async Task AddItemAsync(T item) {
        await _dbSet.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(Expression<Func<T, bool>> predicate, T item) {
        var existing = await GetItemAsync(predicate);
        if (existing != null) {
            _context.Entry(existing).CurrentValues.SetValues(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpsertItemAsync(Expression<Func<T, bool>> predicate, T item) {
        var existing = await GetItemAsync(predicate);
        if (existing == null) {
            await AddItemAsync(item);
        } else {
            _context.Entry(existing).CurrentValues.SetValues(item);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task DeleteItemAsync(Expression<Func<T, bool>> predicate) {
        var item = await GetItemAsync(predicate);
        if (item != null) {
            _dbSet.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}