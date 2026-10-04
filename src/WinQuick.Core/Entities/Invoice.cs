namespace WinQuick.Core.Entities;

public sealed class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid TerminalId { get; set; }
    public Guid UserId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SaleId { get; set; }
    public Guid InvoiceSeriesId { get; set; }
    public required string Number { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public string CurrencyCode { get; set; } = "MZN";
    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsCancelled { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }
}
