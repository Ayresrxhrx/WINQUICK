namespace WinQuick.Core.Entities;

public sealed class Company
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? LegalName { get; set; }
    public string? Nuit { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string CurrencyCode { get; set; } = "MZN";
    public string CurrencySymbol { get; set; } = "MT";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
