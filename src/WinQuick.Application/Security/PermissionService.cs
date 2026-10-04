using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Security;

namespace WinQuick.Application.Security;

public sealed class PermissionService(IRepository<UserRole> userRoles, IRepository<RolePermission> rolePermissions) : IPermissionService
{
    public async Task<bool> HasPermissionAsync(Guid userId, Guid companyId, string permission, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permission)) return false;

        var roleIds = userRoles.Query().Where(x => x.UserId == userId).Select(x => x.RoleId);
        return await rolePermissions.Query()
            .AnyAsync(x => roleIds.Contains(x.RoleId) && x.Permission == permission, cancellationToken);
    }

    public async Task EnsurePermissionAsync(Guid userId, Guid companyId, string permission, CancellationToken cancellationToken = default)
    {
        if (!await HasPermissionAsync(userId, companyId, permission, cancellationToken))
            throw new PermissionDeniedException(permission);
    }
}
