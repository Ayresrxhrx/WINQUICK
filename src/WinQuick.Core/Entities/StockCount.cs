namespace WinQuick.Core.Entities;

public sealed class StockCount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }
    public decimal ExpectedQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal DifferenceQuantity => CountedQuantity - ExpectedQuantity;
    public Guid UserId { get; set; }
    public DateTime CountedAtUtc { get; set; } = DateTime.UtcNow;
}
