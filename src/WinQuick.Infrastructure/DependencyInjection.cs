using Microsoft.Extensions.DependencyInjection;
using WinQuick.Application.Abstractions;
using WinQuick.Infrastructure.Persistence;

namespace WinQuick.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWinQuickInfrastructure(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<DatabaseHealthService>();
        services.AddScoped<TransactionManager>();
        return services;
    }
}
