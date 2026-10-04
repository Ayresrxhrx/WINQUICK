namespace WinQuick.Core.Entities;

public sealed class Terminal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? DeviceIdentifier { get; set; }
    public bool IsServer { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastSeenAtUtc { get; set; }
}
