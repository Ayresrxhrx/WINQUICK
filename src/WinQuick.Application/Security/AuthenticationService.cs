using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;
using CoreRole = WinQuick.Core.Security.Role;
using CoreUserRole = WinQuick.Core.Security.UserRole;

namespace WinQuick.Application.Security;

public sealed record AuthenticatedUser(Guid UserId, Guid CompanyId, string Username, string DisplayName, string RoleName);

public sealed class AuthenticationService(
    IRepository<User> users,
    IRepository<Company> companies,
    IRepository<CoreUserRole> userRoles,
    IRepository<CoreRole> roles,
    IUnitOfWork unitOfWork)
{
    public async Task<AuthenticatedUser?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password)) return null;
        var user = await users.Query().FirstOrDefaultAsync(x => x.Username == username.Trim() && x.IsActive, cancellationToken);
        if (user is null || !VerifyPassword(password, user.PasswordHash)) return null;
        if (!await companies.Query().AnyAsync(x => x.Id == user.CompanyId && x.IsActive, cancellationToken)) return null;

        var roleName = await (from ur in userRoles.Query()
                              join r in roles.Query() on ur.RoleId equals r.Id
                              where ur.UserId == user.Id && r.IsActive
                              select r.Name).FirstOrDefaultAsync(cancellationToken) ?? "Operador";

        user.LastLoginAtUtc = DateTime.UtcNow;
        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new AuthenticatedUser(user.Id, user.CompanyId, user.Username, user.DisplayName ?? user.Username, roleName);
    }

    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) throw new ArgumentException("A senha é obrigatória.");
        Span<byte> salt = stackalloc byte[16];
        RandomNumberGenerator.Fill(salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120_000, HashAlgorithmName.SHA256, 32);
        return $"PBKDF2-SHA256$120000${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string encoded)
    {
        var parts = encoded.Split('$');
        if (parts.Length != 4 || parts[0] != "PBKDF2-SHA256" || !int.TryParse(parts[1], out var iterations)) return false;
        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }
}
