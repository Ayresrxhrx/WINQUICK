using Microsoft.Extensions.DependencyInjection;
using WinQuick.Application.Security;
using WinQuick.Application.Stock;
using WinQuick.Application.Abstractions;
using WinQuick.Infrastructure.Persistence;
using WinQuick.Infrastructure.Stock;

namespace WinQuick.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWinQuickInfrastructure(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<AuthenticationService>();
        services.AddScoped<UserManagementService>();
        services.AddScoped<SecuritySeeder>();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<DatabaseHealthService>();
        services.AddScoped<TransactionManager>();
        return services;
    }
}
