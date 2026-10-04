using Microsoft.EntityFrameworkCore;
using WinQuick.Core.Entities;
using WinQuick.Core.Security;

namespace WinQuick.Infrastructure.Persistence;

public sealed class SecuritySeeder(WinQuickDbContext db)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(cancellationToken)) return;

        var company = await db.Companies.FirstOrDefaultAsync(x => x.IsActive, cancellationToken);
        if (company is null)
        {
            company = new Company { Name = "WINQUICK" };
            db.Companies.Add(company);
        }

        var adminRole = await db.Roles.FirstOrDefaultAsync(x => x.CompanyId == company.Id && x.Name == "Administrador", cancellationToken);
        if (adminRole is null)
        {
            adminRole = new Role { CompanyId = company.Id, Name = "Administrador", Description = "Acesso total ao sistema", IsSystemRole = true, IsActive = true };
            db.Roles.Add(adminRole);
        }

        var admin = new User
        {
            CompanyId = company.Id,
            Username = "Admin",
            DisplayName = "Administrador",
            PasswordHash = "PBKDF2-SHA256$120000$rr6cT6rhtJ8Xf+INd0OFAQ==$jHlogQtWcO7ebaXdI5l/YtCBYbskjWPHXarXGhS7pIo=",
            IsActive = true,
            MustChangePassword = false
        };
        db.Users.Add(admin);
        db.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });
        await db.SaveChangesAsync(cancellationToken);
    }
}
