namespace WinQuick.Core.Entities;

public sealed class SyncOperation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid TerminalId { get; set; }
    public required string IdempotencyKey { get; set; }
    public required string OperationType { get; set; }
    public required string PayloadJson { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
    public string? Error { get; set; }
    public int AttemptCount { get; set; }
}
