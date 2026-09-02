using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Services.Pagination;

public record Page<T>(int Size, IReadOnlyList<T> Items, string NextCursor, string PreviousCursor);

public record Page(int Size, IReadOnlyList<object> Items, string NextCursor, string PreviousCursor)
    : Page<object>(Size, Items, NextCursor, PreviousCursor);

public interface IPaginatorService<T>
{
    public Task<Page> MakePageAsync(
        Cursor cursor,
        int size,
        Func<T, object>? mapperFunc = null,
        CancellationToken cancel = default
    );
    public Task<Page> MakePageAsync(
        string encodedCursor,
        int size,
        Func<T, object>? mapperFunc = null,
        CancellationToken cancel = default
    );
}

public interface IEntityPaginatorService<T> : IPaginatorService<T>
    where T : Entity;
