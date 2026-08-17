using PaydayBackend.Models.Abstractions;
using PaydayBackend.Services.Repositories;

namespace PaydayBackend.Services.Pagination;

public record Page<T>(int Size, IReadOnlyList<T> Items, string NextCursor);

public record Page(int Size, IReadOnlyList<object> Items, string NextCursor)
    : Page<object>(Size, Items, NextCursor);

public interface IPaginatorService<T>
{
    public Task<Page<T>> MakePageAsync(Cursor cursor, int size, CancellationToken cancel = default);
    public Task<Page<T>> MakePageAsync(
        string encodedCursor,
        int size,
        CancellationToken cancel = default
    );
}

public interface IEntityPaginatorService<repositoryType, entityType> : IPaginatorService<entityType>
    where repositoryType : IRepositoryReader<entityType>
    where entityType : Entity
{
    public repositoryType RepositoryReader { get; }
}
