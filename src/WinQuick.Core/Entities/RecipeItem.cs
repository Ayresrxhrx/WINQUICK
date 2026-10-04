namespace WinQuick.Core.Entities;

public sealed class RecipeItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecipeId { get; set; }
    public Guid IngredientId { get; set; }
    public decimal Quantity { get; set; }
}
