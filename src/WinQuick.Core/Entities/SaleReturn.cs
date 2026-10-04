namespace WinQuick.Core.Entities;

public sealed class SaleReturn
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid TerminalId { get; set; }
    public Guid UserId { get; set; }
    public Guid SaleId { get; set; }
    public string Number { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsCancelled { get; set; }
}
