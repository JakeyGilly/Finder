using Finder.Web.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace Finder.Web.Repositories;

public class Repository<T> : IRepository<T> where T : class {
    protected readonly ApplicationContext Context;
    protected Repository(ApplicationContext context) {
        Context = context;
    }
    
    public async Task<T?> GetAsync(ulong id) {
        return await Context.Set<T>().FindAsync((long)id);
    }

    public async Task<IEnumerable<T>> GetAllAsync() {
        return await Context.Set<T>().ToListAsync();
    }

    public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> expression) {
        return await Context.Set<T>().Where(expression).ToListAsync();
    }

    public async Task AddAsync(T entity) {
        await Context.Set<T>().AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities) {
        await Context.Set<T>().AddRangeAsync(entities);
    }

    public void Remove(T entity) {
        Context.Set<T>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities) {
        Context.Set<T>().RemoveRange(entities);
    }
}