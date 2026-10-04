namespace WinQuick.Core.Entities;

public sealed class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid TerminalId { get; set; }
    public Guid UserId { get; set; }
    public Guid? SaleId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal AmountApplied { get; set; }
    public decimal AmountTendered { get; set; }
    public decimal ChangeAmount { get; set; }
    public string? Reference { get; set; }
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsRefund { get; set; }
    public bool IsCancelled { get; set; }
}
