using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Services.Repositories;

public interface IRepositoryReader<T>
    where T : Entity
{
    Task<bool> ExistsAsync(int id, CancellationToken cancel = default);
    Task<T?> GetByIdAsync(int id, CancellationToken cancel = default);

    Task<IReadOnlyList<T>> GetAllAsync(
        int? afterId = null,
        int? beforeId = null,
        int? size = null,
        CancellationToken cancel = default
    );
}

public interface IRepositoryWriter<T>
    where T : Entity
{
    Task CreateAsync(T entity, CancellationToken cancel = default);
    Task<bool> CreateOrReplaceAsync(T entity, CancellationToken cancel = default);
    Task DeleteAsync(int id, CancellationToken cancel = default);
    Task UpdateAsync(int id, object patch, CancellationToken cancel = default);
}

public interface IRepository<T> : IRepositoryReader<T>, IRepositoryWriter<T>
    where T : Entity;
