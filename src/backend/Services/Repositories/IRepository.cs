using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Services.Repositories;

public interface IRepositoryReader<T>
    where T : Entity
{
    Task<bool> ExistsAsync(object id);
    Task<T?> GetByIdAsync(object id, CancellationToken cancel = default);
    Task<IReadOnlyList<T>?> GetAllAsync(
        object? afterId = null,
        int size = 100,
        CancellationToken cancel = default
    );
}

public interface IRepositoryWriter<T>
    where T : Entity
{
    Task CreateAsync(T entity, CancellationToken cancel = default);
    Task CreateOrReplaceAsync(T entity, CancellationToken cancel = default);
    Task DeleteAsync(object id, CancellationToken cancel = default);
    Task UpdateAsync(object id, object patch, CancellationToken cancel = default);
}

public interface IRepository<T> : IRepositoryReader<T>, IRepositoryWriter<T>
    where T : Entity;
