namespace WinQuick.Core.Security;

public sealed class RolePermission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RoleId { get; set; }
    public required string Permission { get; set; }
}
