namespace WinQuick.Application.Abstractions;

public interface IRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> Query();
    ValueTask<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}
