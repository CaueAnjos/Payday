using Microsoft.Extensions.Options;
using PaydayBackend.Models.Abstractions;
using PaydayBackend.Services.Repositories;

namespace PaydayBackend.Services.Pagination;

public class PaginatorServiceOptions
{
    public int MaxPageSize { get; set; }
    public int DefaultPageSize { get; set; }
}

public class EntityPaginatorService<T> : IEntityPaginatorService<T>
    where T : Entity
{
    private readonly PaginatorServiceOptions _options;
    private readonly ICursorService _cursorService;
    private readonly IRepositoryReader<T> _repositoryReader;
    private readonly ILogger<EntityPaginatorService<T>> _logger;

    public EntityPaginatorService(
        IOptions<PaginatorServiceOptions> options,
        IRepositoryReader<T> repository,
        ICursorService cursorService,
        ILogger<EntityPaginatorService<T>> logger
    )
    {
        _options = options.Value;
        _repositoryReader = repository;
        _cursorService = cursorService;
        _logger = logger;
    }

    protected int GetRealSize(int size)
    {
        if (size <= 0 || size > _options.MaxPageSize)
            return _options.DefaultPageSize;

        return size;
    }

    protected Page BuildPage(
        IReadOnlyList<T>? items = null,
        int? afterId = null,
        int? beforeId = null,
        Func<T, object>? mapperFunc = null
    )
    {
        string nextCursor = string.Empty;
        if (afterId.HasValue)
            nextCursor = _cursorService.EncodeCursor(
                new Cursor(afterId.Value, CursorDirection.Forward)
            );

        string previousCursor = string.Empty;
        if (beforeId.HasValue)
            previousCursor = _cursorService.EncodeCursor(
                new Cursor(beforeId.Value, CursorDirection.Backward)
            );

        var pageContent = items?.Select(mapperFunc ?? (i => i)).ToList() ?? [];
        return new Page(Size: pageContent.Count, Items: pageContent, nextCursor, previousCursor);
    }

    public async Task<Page> MakePageAsync(
        string encodeCursor,
        int size = -1,
        Func<T, object>? mapperFunc = null,
        CancellationToken cancel = default
    )
    {
        var cursor = _cursorService.DecodeCursor(encodeCursor);
        if (cursor is null)
        {
            _logger.LogWarning("Cursor couldn't be decoded. So it is not valid.");
            return BuildPage();
        }

        return await MakePageAsync(cursor, size, mapperFunc, cancel);
    }

    public async Task<Page> MakePageAsync(
        Cursor cursor,
        int size = -1,
        Func<T, object>? mapperFunc = null,
        CancellationToken cancel = default
    )
    {
        size = GetRealSize(size);

        return cursor.Direction == CursorDirection.Backward
            ? await MakePreviousPageAsync(cursor.Id, size, mapperFunc, cancel)
            : await MakeNextPageAsync(cursor.Id, size, mapperFunc, cancel);
    }

    private async Task<Page> MakeNextPageAsync(
        int afterId,
        int size,
        Func<T, object>? mapperFunc = null,
        CancellationToken cancel = default
    )
    {
        var entities =
            await _repositoryReader.GetAllAsync(afterId: afterId, size: size + 1, cancel: cancel)
            ?? [];

        var hasMore = entities.Count > size;
        var content = entities.Take(size).ToList();

        int? nextId = hasMore && content.Count > 0 ? content.Last().Id : null;
        int? previousId = afterId > 0 && content.Count > 0 ? content.First().Id : null;

        return BuildPage(content, afterId: nextId, beforeId: previousId, mapperFunc);
    }

    private async Task<Page> MakePreviousPageAsync(
        int beforeId,
        int size,
        Func<T, object>? mapperFunc = null,
        CancellationToken cancel = default
    )
    {
        var entities =
            await _repositoryReader.GetAllAsync(beforeId: beforeId, size: size + 1, cancel: cancel)
            ?? [];

        var hasMore = entities.Count > size;
        var content = entities.TakeLast(size).ToList();

        int? previousId = hasMore && content.Count > 0 ? content.First().Id : null;
        int? nextId = content.Count > 0 ? content.Last().Id : null;

        return BuildPage(content, afterId: nextId, beforeId: previousId);
    }
}
