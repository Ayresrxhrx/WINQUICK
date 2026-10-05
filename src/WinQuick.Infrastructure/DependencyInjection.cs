using Microsoft.Extensions.DependencyInjection;
using WinQuick.Application;
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
        // Register the complete Application layer first. This is required because
        // infrastructure services such as StockService depend on application
        // services (for example IPermissionService and IIngredientService).
        services.AddWinQuickApplication();

        // Infrastructure/persistence services.
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Keep the infrastructure-facing registrations explicit. The concrete
        // implementations are already registered by AddWinQuickApplication(),
        // but these aliases make the infrastructure composition root explicit.
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
