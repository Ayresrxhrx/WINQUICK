namespace WinQuick.Application.Sales;

public interface ISaleService
{
    Task<CreateSaleResult> CreateAsync(CreateSaleCommand command, CancellationToken cancellationToken = default);
}
