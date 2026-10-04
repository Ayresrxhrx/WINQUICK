using Microsoft.EntityFrameworkCore;

namespace WinQuick.Infrastructure.Persistence;

public sealed class DatabaseHealthService(WinQuickDbContext db)
{
    public async Task EnsureHealthyAsync(CancellationToken cancellationToken = default)
    {
        if (!await db.Database.CanConnectAsync(cancellationToken))
            throw new InvalidOperationException("Não foi possível estabelecer ligação à base de dados do WINQUICK.");

        var pending = await db.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pending.Any())
            throw new InvalidOperationException("Existem migrations pendentes. A base de dados deve ser actualizada antes de iniciar operações de produção.");
    }
}
