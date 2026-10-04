namespace WinQuick.Core.Entities;

public sealed class CashMovement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CashSessionId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid TerminalId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public required string Type { get; set; }
    public required string Reason { get; set; }
    public Guid? PaymentId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
