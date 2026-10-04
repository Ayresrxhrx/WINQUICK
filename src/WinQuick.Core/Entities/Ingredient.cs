namespace WinQuick.Core.Entities;

public sealed class Ingredient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public required string Name { get; set; }
    public required string Unit { get; set; }
    public decimal CostPerUnit { get; set; }
    public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; } = true;
}
