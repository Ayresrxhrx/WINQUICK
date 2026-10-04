namespace WinQuick.Core.Entities;

public sealed class BackupRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public required string FilePath { get; set; }
    public long SizeBytes { get; set; }
    public string? Sha256 { get; set; }
    public required string Type { get; set; }
    public bool IsValid { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
