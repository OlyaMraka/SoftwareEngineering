using System.Linq.Expressions;
using KeyKeepers.DAL.Data;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

namespace KeyKeepers.DAL.Repositories.Realizations.Base;

public class RepositoryBase<T> : IRepositoryBase<T>
    where T : class
{
    private readonly KeyKeepersDbContext dbContext;

    protected RepositoryBase(KeyKeepersDbContext context)
    {
        dbContext = context;
    }

    public async Task<IEnumerable<T>> GetAllAsync(QueryOptions<T>? queryOptions = null)
    {
        IQueryable<T> query = dbContext.Set<T>();
        query = ApplyTracking(query, queryOptions?.AsNoTracking ?? true);

        if (queryOptions != null)
        {
            query = ApplyInclude(query, queryOptions.Include);
            query = ApplyFilter(query, queryOptions.Filter);
            query = ApplyPagination(query, queryOptions.Offset, queryOptions.Limit);
        }

        return await query.ToListAsync();
    }

    public async Task<T?> GetFirstOrDefaultAsync(QueryOptions<T>? queryOptions = null)
    {
        IQueryable<T> query = dbContext.Set<T>();
        query = ApplyTracking(query, queryOptions?.AsNoTracking ?? true);

        if (queryOptions != null)
        {
            query = ApplyInclude(query, queryOptions.Include);
            query = ApplyFilter(query, queryOptions.Filter);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<T> CreateAsync(T entity)
    {
        var tmp = await dbContext.Set<T>().AddAsync(entity);
        return tmp.Entity;
    }

    public async Task<int> CountAsync(QueryOptions<T>? queryOptions = null)
    {
        IQueryable<T> query = dbContext.Set<T>();
        query = ApplyTracking(query, queryOptions?.AsNoTracking ?? true);

        if (queryOptions != null)
        {
            query = ApplyFilter(query, queryOptions.Filter);
        }

        return await query.CountAsync();
    }

    public EntityEntry<T> Update(T entity)
    {
        return dbContext.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        dbContext.Set<T>().Remove(entity);
    }

    public async Task<TKey?> MaxAsync<TKey>(
        Expression<Func<T, TKey>> selector,
        Expression<Func<T, bool>>? filter = null)
        where TKey : struct
    {
        var query = dbContext.Set<T>().AsNoTracking();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        var projected = query.Select(selector);

        return await projected.DefaultIfEmpty().MaxAsync();
    }

    public Task<long> CountAsync(Expression<Func<T, bool>> filter)
    {
        return dbContext.Set<T>().LongCountAsync(filter);
    }

    private static IQueryable<T> ApplyFilter(IQueryable<T> query, Expression<Func<T, bool>>? filter)
    {
        return filter is not null ? query.Where(filter) : query;
    }

    private static IQueryable<T> ApplyInclude(IQueryable<T> query, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include)
    {
        return include is not null ? include(query) : query;
    }

    private static IQueryable<T> ApplyPagination(IQueryable<T> query, int offset, int limit)
    {
        if (offset > 0)
        {
            query = query.Skip(offset);
        }

        if (limit > 0)
        {
            query = query.Take(limit);
        }

        return query;
    }

    private IQueryable<T> ApplyTracking(IQueryable<T> query, bool asNoTracking)
    {
        return asNoTracking ? query.AsNoTracking() : query;
    }
}
