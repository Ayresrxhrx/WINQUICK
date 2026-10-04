using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Purchases;

public sealed class PurchaseReceivingService(
    IRepository<PurchaseOrder> orders,
    IRepository<PurchaseOrderItem> items,
    IRepository<StockBalance> balances,
    IRepository<StockMovement> movements,
    IUnitOfWork unitOfWork)
{
    public async Task ReceiveAsync(Guid companyId, Guid purchaseOrderId, Guid userId, Guid terminalId, CancellationToken cancellationToken = default)
    {
        var order = await orders.Query().FirstOrDefaultAsync(x => x.Id == purchaseOrderId && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Pedido de compra não encontrado.");

        if (order.IsCancelled) throw new InvalidOperationException("Não é possível receber uma compra cancelada.");
        if (order.IsReceived) throw new InvalidOperationException("Este pedido de compra já foi recebido.");

        var purchaseItems = await items.Query()
            .Where(x => x.PurchaseOrderId == purchaseOrderId && !x.IsCancelled)
            .ToListAsync(cancellationToken);

        if (purchaseItems.Count == 0) throw new InvalidOperationException("O pedido de compra não possui itens para receber.");

        foreach (var item in purchaseItems)
        {
            if (item.Quantity <= 0m) throw new InvalidOperationException("A quantidade recebida deve ser superior a zero.");

            var balance = await balances.Query().FirstOrDefaultAsync(x => x.CompanyId == companyId && x.ProductId == item.ProductId, cancellationToken);
            if (balance is null)
            {
                balance = new StockBalance
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    ProductId = item.ProductId,
                    Quantity = 0m,
                    ReservedQuantity = 0m,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                await balances.AddAsync(balance, cancellationToken);
            }

            balance.Quantity += item.Quantity;
            balance.UpdatedAtUtc = DateTime.UtcNow;
            balances.Update(balance);

            await movements.AddAsync(new StockMovement
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                ProductId = item.ProductId,
                MovementType = StockMovementType.PurchaseReceipt,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost,
                ReferenceId = purchaseOrderId,
                UserId = userId,
                TerminalId = terminalId,
                CreatedAtUtc = DateTime.UtcNow
            }, cancellationToken);
        }

        order.IsReceived = true;
        order.ReceivedAtUtc = DateTime.UtcNow;
        order.ReceivedByUserId = userId;
        orders.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
