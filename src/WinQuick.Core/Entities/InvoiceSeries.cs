namespace WinQuick.Core.Entities;

public sealed class InvoiceSeries
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public int NextNumber { get; set; } = 1;
    public int NumberLength { get; set; } = 6;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
