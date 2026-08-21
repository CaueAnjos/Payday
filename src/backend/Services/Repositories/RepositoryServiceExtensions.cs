using PaydayBackend.Models;
using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Services.Repositories;

public static class RepositoryServiceExtensions
{
    public static IServiceCollection AddRepository<
        entityType,
        repositoryInterfaceType,
        repositoryType
    >(this IServiceCollection services)
        where entityType : Entity
        where repositoryType : class, IRepository<entityType>, repositoryInterfaceType
        where repositoryInterfaceType : class
    {
        services.AddTransient<repositoryInterfaceType, repositoryType>();
        services.AddTransient<IRepositoryWriter<entityType>, repositoryType>();
        services.AddTransient<IRepositoryReader<entityType>, repositoryType>();
        services.AddTransient<IRepository<entityType>, repositoryType>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddRepository<Payer, IPayersRepository, PayersRepository>();

        return services;
    }
}
