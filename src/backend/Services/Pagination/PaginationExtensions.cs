using PaydayBackend.Models;
using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Services.Pagination;

public static class PaginationExtensions
{
    public static IServiceCollection AddEntityPaginator<entityType>(
        this IServiceCollection services
    )
        where entityType : Entity
    {
        services.AddTransient<IPaginatorService<entityType>, EntityPaginatorService<entityType>>();
        services.AddTransient<
            IEntityPaginatorService<entityType>,
            EntityPaginatorService<entityType>
        >();

        return services;
    }

    public static IServiceCollection AddPagination(
        this IServiceCollection services,
        Action<PaginatorServiceOptions> options
    )
    {
        services.Configure<PaginatorServiceOptions>(options);

        services.AddDataProtection();
        services.AddTransient<ICursorService, CursorService>();

        services.AddEntityPaginator<Payer>();
        services.AddEntityPaginator<Contract>();

        return services;
    }
}
