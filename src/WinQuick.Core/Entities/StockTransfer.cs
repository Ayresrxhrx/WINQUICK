namespace WinQuick.Core.Entities;

public sealed class StockTransfer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public required string SourceLocation { get; set; }
    public required string DestinationLocation { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
