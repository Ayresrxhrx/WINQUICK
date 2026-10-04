namespace WinQuick.Core.Entities;

public sealed class StockBalance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public decimal AvailableQuantity => Quantity - ReservedQuantity;
}
