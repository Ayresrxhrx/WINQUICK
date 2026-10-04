namespace WinQuick.Core.Entities;

public sealed class CreditNote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid TerminalId { get; set; }
    public Guid UserId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OriginalInvoiceId { get; set; }
    public Guid InvoiceSeriesId { get; set; }
    public required string Number { get; set; }
    public required string Reason { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsCancelled { get; set; }
}
