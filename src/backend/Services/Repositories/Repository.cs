using Microsoft.EntityFrameworkCore;
using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Services.Repositories;

public abstract class RepositoryBase<T>(DbContext context) : IRepository<T>
    where T : Entity
{
    private readonly DbSet<T> _entities = context.Set<T>();

    protected async Task<bool> ShouldCreateAsync(int? id)
    {
        if (!id.HasValue || await ExistsAsync(id.Value))
            return false;
        return true;
    }

    protected async Task<bool> ShouldDeleteAsync(int? id)
    {
        if (!id.HasValue || await ExistsAsync(id.Value))
            return true;
        return false;
    }

    protected async Task<bool> ShouldUpdateAsync(int? id)
    {
        if (!id.HasValue || await ExistsAsync(id.Value))
            return true;
        return false;
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        var entity = await _entities.FindAsync(id);
        return entity is not null;
    }

    public virtual async Task CreateAsync(T entity, CancellationToken cancel = default)
    {
        await _entities.AddAsync(entity, cancel);
        await context.SaveChangesAsync(cancel);
    }

    public virtual async Task CreateOrReplaceAsync(T entity, CancellationToken cancel = default)
    {
        if (await ShouldCreateAsync(entity.Id))
            await CreateAsync(entity);
        else
            await UpdateAsync(entity.Id, entity, cancel);
    }

    public virtual async Task DeleteAsync(int id, CancellationToken cancel = default)
    {
        var entity = await GetByIdAsync(id);
        if (await ShouldDeleteAsync(id) && entity is not null)
        {
            _entities.Remove(entity);
            await context.SaveChangesAsync();
        }
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

    public virtual async Task UpdateAsync(int id, object patch, CancellationToken cancel = default)
    {
        var enityToUpdate = await GetByIdAsync(id);
        if (await ShouldUpdateAsync(id) && enityToUpdate is not null)
        {
            context.Entry(enityToUpdate).CurrentValues.SetValues(patch);
            await context.SaveChangesAsync();
        }
    }
}
