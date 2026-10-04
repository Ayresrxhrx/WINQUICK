using WinQuick.Core.Entities;

namespace WinQuick.Application.Purchases;

public interface IPurchaseService
{
    Task<PurchaseOrder> CreateAsync(CreatePurchaseCommand command, CancellationToken cancellationToken = default);
    Task<PurchaseOrder> ReceiveAsync(ReceivePurchaseCommand command, CancellationToken cancellationToken = default);
}

public sealed record CreatePurchaseItem(Guid ProductId, decimal Quantity, decimal UnitCost, decimal TaxRate);
public sealed record CreatePurchaseCommand(Guid CompanyId, Guid SupplierId, Guid UserId, IReadOnlyCollection<CreatePurchaseItem> Items);
public sealed record ReceivePurchaseItem(Guid PurchaseOrderItemId, decimal Quantity);
public sealed record ReceivePurchaseCommand(Guid CompanyId, Guid UserId, Guid? TerminalId, Guid PurchaseOrderId, IReadOnlyCollection<ReceivePurchaseItem> Items);
