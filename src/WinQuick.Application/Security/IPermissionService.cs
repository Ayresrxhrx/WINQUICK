namespace WinQuick.Application.Security;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, Guid companyId, string permission, CancellationToken cancellationToken = default);
    Task EnsurePermissionAsync(Guid userId, Guid companyId, string permission, CancellationToken cancellationToken = default);
}
