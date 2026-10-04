using Microsoft.EntityFrameworkCore.Storage;

namespace WinQuick.Infrastructure.Persistence;

public sealed class TransactionManager(WinQuickDbContext db)
{
    public Task<IDbContextTransaction> BeginAsync(CancellationToken cancellationToken = default)
        => db.Database.BeginTransactionAsync(cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}
