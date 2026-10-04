namespace WinQuick.Core.Entities;

public sealed class StockMovement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? TerminalId { get; set; }
    public Guid UserId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public required string Type { get; set; }
    public string? Reference { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
