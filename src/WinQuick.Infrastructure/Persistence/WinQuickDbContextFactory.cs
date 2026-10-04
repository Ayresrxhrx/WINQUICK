using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WinQuick.Infrastructure.Persistence;

public sealed class WinQuickDbContextFactory : IDesignTimeDbContextFactory<WinQuickDbContext>
{
    public WinQuickDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<WinQuickDbContext>()
            .UseSqlite("Data Source=winquick.db")
            .Options;

        return new WinQuickDbContext(options);
    }
}
