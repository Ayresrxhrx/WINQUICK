using Microsoft.EntityFrameworkCore;

namespace WinQuick.Infrastructure.Persistence;

public sealed class DatabaseInitializer(WinQuickDbContext db, SecuritySeeder securitySeeder)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);
        await securitySeeder.SeedAsync(cancellationToken);
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => db.Database.CanConnectAsync(cancellationToken);
}
