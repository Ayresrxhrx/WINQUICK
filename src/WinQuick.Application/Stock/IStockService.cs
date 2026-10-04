namespace WinQuick.Application.Stock;

public interface IStockService
{
    Task DecreaseAsync(
        Guid companyId,
        Guid productId,
        decimal quantity,
        Guid userId,
        Guid terminalId,
        string reference,
        CancellationToken cancellationToken = default);
}
