using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Services.Repositories;

public interface IRepository<T>
    where T : Entity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancel = default);
    Task<ICollection<T>?> GetAllAsync(
        int afterId = 0,
        int size = 100,
        CancellationToken cancel = default
    );
    Task<bool> CreateAsync(T entity, CancellationToken cancel = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancel = default);
    Task<bool> UpdateAsync(int id, object patch, CancellationToken cancel = default);
}
