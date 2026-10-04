using Microsoft.Extensions.DependencyInjection;
using WinQuick.Application.Sales;

namespace WinQuick.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddWinQuickApplication(this IServiceCollection services)
    {
        services.AddScoped<ISaleService, SaleService>();
        return services;
    }
}
