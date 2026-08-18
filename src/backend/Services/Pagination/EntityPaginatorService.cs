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
        if (size < 0 || size > _options.MaxPageSize)
            return _options.DefaultPageSize;

        return size;
    }

    protected Page<entityType> BuildPage(
        IReadOnlyList<entityType>? items = null,
        int? afterId = null
    )
    {
        string nextCursor = string.Empty;
        if (afterId.HasValue)
            nextCursor = _cursorService.EncodeCursor(new Cursor(afterId.Value));

        var pageContent = items ?? [];
        return new Page<entityType>(Size: pageContent.Count, Items: pageContent, nextCursor);
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

        return await MakePageAsync(new Cursor(cursor.Id), size, cancel);
    }

    public async Task<Page<entityType>> MakePageAsync(
        Cursor cursor,
        int size = -1,
        CancellationToken cancel = default
    )
    {
        size = GetRealSize(size);
        int? afterId = cursor.Id;

        var entities = await _repositoryReader.GetAllAsync(afterId!, size + 1, cancel);
        if (entities is not null && entities.Count > size)
            afterId = entities.ElementAt(size - 1).Id;
        else
            afterId = null;

        if (entities is null)
        {
            _logger.LogError("entities is null");
        }

        var content = entities?.Take(size).ToList();
        if (content is null)
        {
            _logger.LogError("content is null");
        }

        return BuildPage(content, afterId);
    }
}
