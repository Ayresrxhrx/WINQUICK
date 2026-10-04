using Microsoft.Extensions.DependencyInjection;

namespace WinQuick.Application.Ingredients;

public static class IngredientDependencyInjection
{
    public static IServiceCollection AddWinQuickIngredients(this IServiceCollection services)
    {
        services.AddScoped<IIngredientService, IngredientService>();
        return services;
    }
}
