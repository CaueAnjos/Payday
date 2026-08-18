using Microsoft.EntityFrameworkCore;
using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Services.Repositories;

public abstract class RepositoryBase<T>(DbContext context) : IRepository<T>
    where T : Entity
{
    private readonly DbSet<T> _entities = context.Set<T>();

    protected async Task<bool> ShouldCreateAsync(int id)
    {
        if (await ExistsAsync(id))
            return false;
        return true;
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        var entity = await _entities.FindAsync();
        return entity is not null;
    }

    public virtual async Task CreateAsync(T entity, CancellationToken cancel = default)
    {
        await _entities.AddAsync(entity, cancel);
        await context.SaveChangesAsync(cancel);
    }

    public virtual async Task CreateOrReplaceAsync(T entity, CancellationToken cancel = default)
    {
        if (await _entities.FindAsync(entity.Id) is not null) { }
        await UpdateAsync(entity.Id, entity, cancel);
        await CreateAsync(entity);
    }

    public virtual async Task DeleteAsync(int id, CancellationToken cancel = default)
    {
        var entity = await _entities.FindAsync(id);
        if (entity is null)
            return;

        _entities.Remove(entity);
        await context.SaveChangesAsync();
    }

    public virtual async Task<IReadOnlyList<T>?> GetAllAsync(
        int? afterId = null,
        int size = 100,
        CancellationToken cancel = default
    )
    {
        if (size <= 0)
            return null;

        if (afterId is int cursor)
        {
            return await _entities
                .Where(e => e.Id > cursor)
                .OrderBy(p => p.Id)
                .Take(size)
                .AsNoTracking()
                .ToListAsync(cancel);
        }

        return null;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancel = default)
    {
        return await _entities.FindAsync(id);
    }

    public virtual async Task UpdateAsync(int id, object patch, CancellationToken cancel)
    {
        var enityToUpdate = await _entities.FindAsync(id);
        if (enityToUpdate is null)
            return;

        context.Entry(enityToUpdate).CurrentValues.SetValues(patch);
        await context.SaveChangesAsync();
    }
}
