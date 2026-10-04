namespace WinQuick.Core.Entities;

public sealed class CashSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid TerminalId { get; set; }
    public Guid UserId { get; set; }
    public decimal OpeningAmount { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal CountedAmount { get; set; }
    public decimal DifferenceAmount { get; set; }
    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAtUtc { get; set; }
    public bool IsOpen { get; set; } = true;
    public string? ClosingJustification { get; set; }
}
