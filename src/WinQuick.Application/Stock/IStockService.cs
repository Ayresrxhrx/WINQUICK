namespace WinQuick.Application.Stock;

public interface IStockService
{
    Task DecreaseAsync(Guid companyId, Guid productId, decimal quantity, Guid userId, Guid terminalId, string reference, CancellationToken cancellationToken = default);
    Task IncreaseAsync(Guid companyId, Guid productId, decimal quantity, Guid userId, Guid? terminalId, string reference, CancellationToken cancellationToken = default);
    Task AdjustAsync(Guid companyId, Guid productId, decimal targetQuantity, Guid userId, Guid? terminalId, string reason, CancellationToken cancellationToken = default);
}
