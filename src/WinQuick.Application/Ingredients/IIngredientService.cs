using WinQuick.Core.Entities;

namespace WinQuick.Application.Ingredients;

public interface IIngredientService
{
    Task ConsumeForSaleAsync(Guid companyId, Guid productId, decimal productQuantity, Guid userId, Guid? terminalId, string reference, CancellationToken cancellationToken = default);
    Task<Ingredient> CreateAsync(CreateIngredientCommand command, CancellationToken cancellationToken = default);
}

public sealed record CreateIngredientCommand(Guid CompanyId, string Name, string Unit, decimal CostPerUnit, decimal MinimumStock);
