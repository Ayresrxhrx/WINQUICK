using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;

namespace WinQuick.Infrastructure.Persistence;

public sealed class Repository<TEntity>(WinQuickDbContext db) : IRepository<TEntity> where TEntity : class
{
    private readonly DbSet<TEntity> _set = db.Set<TEntity>();

    public IQueryable<TEntity> Query() => _set.AsQueryable();

    public ValueTask<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _set.FindAsync([id], cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await _set.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => _set.Update(entity);

    public void Remove(TEntity entity) => _set.Remove(entity);
}
