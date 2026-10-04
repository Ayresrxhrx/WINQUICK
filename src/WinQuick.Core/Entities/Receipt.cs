namespace WinQuick.Core.Entities;

public sealed class Receipt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid TerminalId { get; set; }
    public Guid UserId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? InvoiceId { get; set; }
    public required string Number { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsCancelled { get; set; }
}
