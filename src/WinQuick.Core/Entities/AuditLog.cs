namespace WinQuick.Core.Entities;

public sealed class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? TerminalId { get; set; }
    public required string Action { get; set; }
    public required string EntityName { get; set; }
    public Guid? EntityId { get; set; }
    public string? DetailsJson { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
