using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Ingredients;

public sealed class IngredientService(
    IRepository<Ingredient> ingredients,
    IRepository<Recipe> recipes,
    IRepository<RecipeItem> recipeItems,
    IRepository<IngredientStockBalance> balances,
    IUnitOfWork unitOfWork) : IIngredientService
{
    public async Task<Ingredient> CreateAsync(CreateIngredientCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name)) throw new ArgumentException("O ingrediente deve possuir nome.");
        if (string.IsNullOrWhiteSpace(command.Unit)) throw new ArgumentException("A unidade do ingrediente é obrigatória.");
        if (command.CostPerUnit < 0 || command.MinimumStock < 0) throw new ArgumentException("Valores de custo e stock inválidos.");

        var ingredient = new Ingredient
        {
            CompanyId = command.CompanyId,
            Name = command.Name.Trim(),
            Unit = command.Unit.Trim(),
            CostPerUnit = command.CostPerUnit,
            MinimumStock = command.MinimumStock,
            IsActive = true
        };

        await ingredients.AddAsync(ingredient, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ingredient;
    }

    public async Task ConsumeForSaleAsync(Guid companyId, Guid productId, decimal productQuantity, Guid userId, Guid? terminalId, string reference, CancellationToken cancellationToken = default)
    {
        if (productQuantity <= 0) throw new ArgumentException("A quantidade do produto deve ser maior que zero.");

        var recipe = recipes.Query().FirstOrDefault(x => x.CompanyId == companyId && x.ProductId == productId && x.IsActive)
            ?? throw new InvalidOperationException("O produto composto não possui uma receita activa.");

        var items = recipeItems.Query().Where(x => x.RecipeId == recipe.Id).ToList();
        if (items.Count == 0) throw new InvalidOperationException("A receita não possui ingredientes.");

        foreach (var item in items)
        {
            var required = item.Quantity * productQuantity;
            var balance = balances.Query().FirstOrDefault(x => x.CompanyId == companyId && x.IngredientId == item.IngredientId);
            if (balance is null || balance.Quantity < required)
                throw new InvalidOperationException($"Stock insuficiente do ingrediente {item.IngredientId}.");

            balance.Quantity -= required;
            balance.UpdatedAtUtc = DateTime.UtcNow;
            balances.Update(balance);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
