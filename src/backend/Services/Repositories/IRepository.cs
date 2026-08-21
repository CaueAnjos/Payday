using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Services.Repositories;

public interface IRepositoryReader<T>
    where T : Entity
{
    Task<bool> ExistsAsync(int id);
    Task<T?> GetByIdAsync(int id, CancellationToken cancel = default);

    /// <summary>
    /// Fetches a page of entities ordered by Id. Only one of <paramref name="afterId"/> or
    /// <paramref name="beforeId"/> should be provided at a time; <paramref name="afterId"/> pages
    /// forward (Id &gt; afterId, ascending) and <paramref name="beforeId"/> pages backward
    /// (Id &lt; beforeId), but the returned list is always ordered ascending by Id.
    /// </summary>
    Task<IReadOnlyList<T>?> GetAllAsync(
        int? afterId = null,
        int? beforeId = null,
        int size = 100,
        CancellationToken cancel = default
    );
}

public interface IRepositoryWriter<T>
    where T : Entity
{
    Task CreateAsync(T entity, CancellationToken cancel = default);

    /// <summary>
    /// Creates the entity if it doesn't already exist, otherwise replaces it.
    /// </summary>
    /// <returns><c>true</c> if a new entity was created; <c>false</c> if an existing one was replaced.</returns>
    Task<bool> CreateOrReplaceAsync(T entity, CancellationToken cancel = default);
    Task DeleteAsync(int id, CancellationToken cancel = default);
    Task UpdateAsync(int id, object patch, CancellationToken cancel = default);
}

public interface IRepository<T> : IRepositoryReader<T>, IRepositoryWriter<T>
    where T : Entity;
