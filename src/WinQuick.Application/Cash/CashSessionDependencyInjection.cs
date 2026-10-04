using Microsoft.Extensions.DependencyInjection;

namespace WinQuick.Application.Cash;

public static class CashSessionDependencyInjection
{
    public static IServiceCollection AddWinQuickCash(this IServiceCollection services)
    {
        services.AddScoped<ICashSessionService, CashSessionService>();
        return services;
    }
}
