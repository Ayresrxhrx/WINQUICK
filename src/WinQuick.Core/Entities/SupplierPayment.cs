namespace WinQuick.Core.Entities;

public sealed class SupplierPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid SupplierId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public required string Reference { get; set; }
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
}
