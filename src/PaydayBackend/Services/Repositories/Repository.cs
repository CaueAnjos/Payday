using Microsoft.EntityFrameworkCore;
using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Services.Repositories;

public abstract class RepositoryBase<T>(DbContext context) : IRepository<T>
    where T : Entity
{
    private readonly DbSet<T> _entities = context.Set<T>();

    protected IQueryable<T> _query => AddIncludes(_entities.AsQueryable());

    protected virtual IQueryable<T> AddIncludes(IQueryable<T> query)
    {
        return query.AsNoTracking();
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _query.AnyAsync(e => e.Id == id);
    }

    public virtual async Task CreateAsync(T entity, CancellationToken cancel = default)
    {
        await _entities.AddAsync(entity, cancel);
        await context.SaveChangesAsync(cancel);
    }

    public virtual async Task<bool> CreateOrReplaceAsync(
        T entity,
        CancellationToken cancel = default
    )
    {
        var exists = await ExistsAsync(entity.Id);
        if (exists)
            await UpdateAsync(entity.Id, entity, cancel);
        else
            await CreateAsync(entity, cancel);

        return !exists;
    }

    public virtual async Task DeleteAsync(int id, CancellationToken cancel = default)
    {
        var entity = await GetByIdAsync(id, cancel);
        if (entity is not null)
        {
            _entities.Remove(entity);
            await context.SaveChangesAsync(cancel);
        }
    }

    public virtual async Task<IReadOnlyList<T>?> GetAllAsync(
        int? afterId = null,
        int? beforeId = null,
        int size = 100,
        CancellationToken cancel = default
    )
    {
        if (size <= 0)
            return null;

        if (beforeId is int before)
        {
            var precedingEntities = await _query
                .Where(e => e.Id < before)
                .OrderByDescending(e => e.Id)
                .Take(size)
                .ToListAsync(cancel);

            precedingEntities.Reverse();
            return precedingEntities;
        }

        var cursor = afterId ?? 0;

        return await _query
            .Where(e => e.Id > cursor)
            .OrderBy(e => e.Id)
            .Take(size)
            .ToListAsync(cancel);
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancel = default)
    {
        return await _query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task UpdateAsync(int id, object patch, CancellationToken cancel = default)
    {
        var entityToUpdate = await GetByIdAsync(id, cancel);
        if (entityToUpdate is not null)
        {
            context.Entry(entityToUpdate).CurrentValues.SetValues(patch);
            await context.SaveChangesAsync(cancel);
        }
    }
}
