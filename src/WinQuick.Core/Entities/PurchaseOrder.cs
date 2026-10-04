namespace WinQuick.Core.Entities;

public sealed class PurchaseOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid SupplierId { get; set; }
    public Guid UserId { get; set; }
    public required string Number { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReceivedAtUtc { get; set; }
    public bool IsCancelled { get; set; }
}
