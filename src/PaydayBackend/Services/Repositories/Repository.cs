using Microsoft.EntityFrameworkCore;
using PaydayBackend.Exceptions;
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

    public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancel = default)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException("Only positive `id` are allowed");

        return await _query.AnyAsync(e => e.Id == id);
    }

    public virtual async Task CreateAsync(T entity, CancellationToken cancel = default)
    {
        if (await ExistsAsync(entity.Id))
        {
            throw new DuplicateEntityException(entity);
        }

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
        else
        {
            throw new EntityNotFoundException(typeof(T), id);
        }
    }

    // TODO: this shouldn't be just one method
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(
        int? afterId = null,
        int? beforeId = null,
        int? size = null,
        CancellationToken cancel = default
    )
    {
        if (size < 0)
            throw new ArgumentOutOfRangeException("Only positive `size` are allowed");

        if (afterId < 0)
            throw new ArgumentOutOfRangeException("Only positive `afterId` are allowed");

        if (beforeId < 0)
            throw new ArgumentOutOfRangeException("Only positive `beforeId` are allowed");

        if (size == 0)
            return [];

        IQueryable<T> query = _query;

        if (afterId is not null)
            query = query.Where(e => e.Id > afterId.Value);

        if (beforeId is not null)
            query = query.Where(e => e.Id < beforeId.Value);

        query = beforeId is not null
            ? query.OrderByDescending(e => e.Id)
            : query.OrderBy(e => e.Id);

        if (size.HasValue)
            query = query.Take(size.Value);

        var queryResult = await query.ToListAsync(cancel);

        if (beforeId.HasValue)
            queryResult.Reverse();

        return queryResult;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancel = default)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException("Only positive `id` are allowed");

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
        else
        {
            throw new EntityNotFoundException(typeof(T), id);
        }
    }
}
