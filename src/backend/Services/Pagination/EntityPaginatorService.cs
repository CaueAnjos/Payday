using Microsoft.Extensions.Options;
using PaydayBackend.Models.Abstractions;
using PaydayBackend.Services.Repositories;

namespace PaydayBackend.Services.Pagination;

public class PaginatorServiceOptions
{
    public int MaxPageSize { get; set; }
    public int DefaultPageSize { get; set; }
}

public class EntityPaginatorService<entityType, repositoryType>
    : IEntityPaginatorService<repositoryType, entityType>
    where entityType : Entity
    where repositoryType : IRepositoryReader<entityType>
{
    private readonly PaginatorServiceOptions _options;
    private readonly ICursorService _cursorService;
    private readonly repositoryType _repositoryReader;
    private readonly ILogger<EntityPaginatorService<entityType, repositoryType>> _logger;

    public repositoryType RepositoryReader => _repositoryReader;

    public EntityPaginatorService(
        IOptions<PaginatorServiceOptions> options,
        repositoryType repository,
        ICursorService cursorService,
        ILogger<EntityPaginatorService<entityType, repositoryType>> logger
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

    protected Page<entityType> BuildPage(
        IReadOnlyList<entityType>? items = null,
        int? afterId = null,
        int? beforeId = null
    )
    {
        string nextCursor = string.Empty;
        if (afterId.HasValue)
            nextCursor = _cursorService.EncodeCursor(new Cursor(afterId.Value, CursorDirection.Forward));

        string previousCursor = string.Empty;
        if (beforeId.HasValue)
            previousCursor = _cursorService.EncodeCursor(
                new Cursor(beforeId.Value, CursorDirection.Backward)
            );

        var pageContent = items ?? [];
        return new Page<entityType>(
            Size: pageContent.Count,
            Items: pageContent,
            nextCursor,
            previousCursor
        );
    }

    public async Task<Page<entityType>> MakePageAsync(
        string encodeCursor,
        int size = -1,
        CancellationToken cancel = default
    )
    {
        var cursor = _cursorService.DecodeCursor(encodeCursor);
        if (cursor is null)
        {
            _logger.LogWarning("Cursor couldn't be decoded. So it is not valid.");
            return BuildPage();
        }

        return await MakePageAsync(cursor, size, cancel);
    }

    public async Task<Page<entityType>> MakePageAsync(
        Cursor cursor,
        int size = -1,
        CancellationToken cancel = default
    )
    {
        size = GetRealSize(size);

        return cursor.Direction == CursorDirection.Backward
            ? await MakePreviousPageAsync(cursor.Id, size, cancel)
            : await MakeNextPageAsync(cursor.Id, size, cancel);
    }

    private async Task<Page<entityType>> MakeNextPageAsync(
        int afterId,
        int size,
        CancellationToken cancel
    )
    {
        var entities =
            await _repositoryReader.GetAllAsync(afterId: afterId, size: size + 1, cancel: cancel)
            ?? [];

        var hasMore = entities.Count > size;
        var content = entities.Take(size).ToList();

        int? nextId = hasMore && content.Count > 0 ? content.Last().Id : null;
        int? previousId = afterId > 0 && content.Count > 0 ? content.First().Id : null;

        return BuildPage(content, afterId: nextId, beforeId: previousId);
    }

    private async Task<Page<entityType>> MakePreviousPageAsync(
        int beforeId,
        int size,
        CancellationToken cancel
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
