using Microsoft.EntityFrameworkCore;

namespace WinQuick.Infrastructure.Persistence;

public sealed class DatabaseInitializer(WinQuickDbContext db)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => db.Database.CanConnectAsync(cancellationToken);
}
