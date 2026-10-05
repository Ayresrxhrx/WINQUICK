using Microsoft.Extensions.DependencyInjection;
using WinQuick.Application.Abstractions;
using WinQuick.Application.Products;
using WinQuick.Application.Security;
using WinQuick.Application.Sales;
using WinQuick.Infrastructure.Persistence;

namespace WinQuick.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWinQuickInfrastructure(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IStockService, WinQuick.Application.Stock.StockService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<AuthenticationService>();
        services.AddScoped<UserManagementService>();
        services.AddScoped<SecuritySeeder>();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<DatabaseHealthService>();
        services.AddScoped<TransactionManager>();
        return services;
    }
}
