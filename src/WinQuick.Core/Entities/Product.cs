namespace WinQuick.Core.Entities;

public sealed class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? TaxRateId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Sku { get; set; }
    public string? Unit { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal MaximumStock { get; set; }
    public bool TrackStock { get; set; } = true;
    public bool IsComposite { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
