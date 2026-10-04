using Microsoft.Extensions.DependencyInjection;

namespace WinQuick.Application.Cash;

public static class CashDependencyInjection
{
    public static IServiceCollection AddWinQuickCashServices(this IServiceCollection services)
    {
        services.AddScoped<ICashService, CashService>();
        return services;
    }
}
