using Microsoft.EntityFrameworkCore;
using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Services.Repositories;

public abstract class RepositoryBase<T>(DbContext context) : IRepository<T>
    where T : Entity
{
    private readonly DbSet<T> _entities = context.Set<T>();
    public readonly int MaxPageSize = 1000;

    public virtual async Task<bool> CreateAsync(T entity, CancellationToken cancel = default)
    {
        await _entities.AddAsync(entity, cancel);
        return await context.SaveChangesAsync(cancel) > 0;
    }

    public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancel = default)
    {
        var entity = await _entities.FindAsync(id);
        if (entity is null)
            return false;

        _entities.Remove(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public virtual async Task<ICollection<T>?> GetAllAsync(
        int afterId = 0,
        int size = 100,
        CancellationToken cancel = default
    )
    {
        if (size <= 0)
            return null;

        return await _entities
            .Where(e => e.Id > afterId)
            .OrderBy(p => p.Id)
            .Take(size)
            .AsNoTracking()
            .ToListAsync(cancel);
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancel = default)
    {
        return await _entities.FindAsync(id);
    }

    public virtual async Task<bool> UpdateAsync(int id, object patch, CancellationToken cancel)
    {
        var enityToUpdate = await _entities.FindAsync(id);
        if (enityToUpdate is null)
            return false;

        context.Entry(enityToUpdate).CurrentValues.SetValues(patch);
        return await context.SaveChangesAsync() > 0;
    }
}
