namespace WinQuick.Core.Entities;

public sealed class ProductBarcode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public required string Barcode { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
}
