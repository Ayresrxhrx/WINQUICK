namespace WinQuick.Core.Entities;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid? EmployeeId { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public string? PinHash { get; set; }
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAtUtc { get; set; }
}
