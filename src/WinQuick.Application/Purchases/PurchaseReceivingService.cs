using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Purchases;

public sealed class PurchaseReceivingService(IRepository<PurchaseOrder> orders, IRepository<PurchaseOrderItem> items, IRepository<StockBalance> balances, IRepository<StockMovement> movements, IUnitOfWork unitOfWork)
{
    public async Task ReceiveAsync(Guid companyId, Guid purchaseOrderId, Guid userId, Guid terminalId, CancellationToken cancellationToken = default)
    {
        var order = await orders.Query().FirstOrDefaultAsync(x => x.Id == purchaseOrderId && x.CompanyId == companyId, cancellationToken) ?? throw new InvalidOperationException("Pedido de compra não encontrado.");
        if (order.IsCancelled) throw new InvalidOperationException("Não é possível receber uma compra cancelada.");
        if (order.ReceivedAtUtc is not null) throw new InvalidOperationException("Este pedido de compra já foi recebido.");
        var purchaseItems = await items.Query().Where(x => x.PurchaseOrderId == purchaseOrderId).ToListAsync(cancellationToken);
        if (purchaseItems.Count == 0) throw new InvalidOperationException("O pedido de compra não possui itens para receber.");

        foreach (var item in purchaseItems)
        {
            var remaining = item.OrderedQuantity - item.ReceivedQuantity;
            if (remaining <= 0m) continue;
            var balance = await balances.Query().FirstOrDefaultAsync(x => x.CompanyId == companyId && x.ProductId == item.ProductId, cancellationToken);
            if (balance is null)
            {
                balance = new StockBalance { CompanyId = companyId, ProductId = item.ProductId, Quantity = 0m, ReservedQuantity = 0m, UpdatedAtUtc = DateTime.UtcNow };
                await balances.AddAsync(balance, cancellationToken);
            }
            balance.Quantity += remaining;
            balance.UpdatedAtUtc = DateTime.UtcNow;
            balances.Update(balance);
            item.ReceivedQuantity += remaining;
            await movements.AddAsync(new StockMovement { CompanyId = companyId, ProductId = item.ProductId, TerminalId = terminalId, UserId = userId, Quantity = remaining, UnitCost = item.UnitCost, Type = "PurchaseReceipt", Reference = purchaseOrderId.ToString(), CreatedAtUtc = DateTime.UtcNow }, cancellationToken);
        }

        order.ReceivedAtUtc = DateTime.UtcNow;
        orders.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
