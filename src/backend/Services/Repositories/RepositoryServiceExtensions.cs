namespace PaydayBackend.Services.Repositories;

public static class RespositoryServiceExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<IPayersRespository, PayersRespository>();

        return services;
    }
}
