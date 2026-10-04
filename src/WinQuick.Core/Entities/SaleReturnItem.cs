namespace WinQuick.Core.Entities;

public sealed class SaleReturnItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SaleReturnId { get; set; }
    public Guid SaleItemId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
