namespace WinQuick.Application.Abstractions;

public interface IStockService
{
    Task DecreaseAsync(Guid companyId, Guid terminalId, Guid productId, decimal quantity, decimal unitCost, string reference, CancellationToken cancellationToken = default);
    Task IncreaseAsync(Guid companyId, Guid terminalId, Guid productId, decimal quantity, decimal unitCost, string reference, CancellationToken cancellationToken = default);
}
