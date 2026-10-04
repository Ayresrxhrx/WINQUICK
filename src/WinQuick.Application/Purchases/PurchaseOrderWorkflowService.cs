using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Purchases;

public sealed record AddPurchaseItemCommand(Guid CompanyId, Guid PurchaseOrderId, Guid ProductId, decimal Quantity, decimal UnitCost, decimal TaxRate = 0m);

public sealed class PurchaseOrderWorkflowService(IRepository<PurchaseOrder> orders, IRepository<PurchaseOrderItem> items, IRepository<Product> products, IUnitOfWork unitOfWork)
{
    public async Task<PurchaseOrderItem> AddItemAsync(AddPurchaseItemCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Quantity <= 0m) throw new ArgumentException("A quantidade deve ser superior a zero.");
        if (command.UnitCost < 0m || command.TaxRate < 0m) throw new ArgumentException("Os valores da compra não podem ser negativos.");
        var order = await orders.Query().FirstOrDefaultAsync(x => x.Id == command.PurchaseOrderId && x.CompanyId == command.CompanyId, cancellationToken) ?? throw new InvalidOperationException("Pedido de compra não encontrado.");
        var product = await products.Query().FirstOrDefaultAsync(x => x.Id == command.ProductId && x.CompanyId == command.CompanyId, cancellationToken) ?? throw new InvalidOperationException("Produto não encontrado.");
        if (order.IsCancelled || order.ReceivedAtUtc is not null) throw new InvalidOperationException("Não é possível alterar um pedido de compra cancelado ou já recebido.");
        var taxAmount = Math.Round(command.Quantity * command.UnitCost * command.TaxRate / 100m, 2, MidpointRounding.AwayFromZero);
        var item = new PurchaseOrderItem { PurchaseOrderId = order.Id, ProductId = product.Id, OrderedQuantity = command.Quantity, ReceivedQuantity = 0m, UnitCost = command.UnitCost, TaxRate = command.TaxRate, LineTotal = command.Quantity * command.UnitCost + taxAmount };
        await items.AddAsync(item, cancellationToken);
        await RecalculateAsync(order.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task RecalculateAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default)
    {
        var order = await orders.Query().FirstOrDefaultAsync(x => x.Id == purchaseOrderId, cancellationToken) ?? throw new InvalidOperationException("Pedido de compra não encontrado.");
        var lines = await items.Query().Where(x => x.PurchaseOrderId == purchaseOrderId).ToListAsync(cancellationToken);
        order.Subtotal = lines.Sum(x => x.OrderedQuantity * x.UnitCost);
        order.TaxAmount = lines.Sum(x => Math.Round(x.OrderedQuantity * x.UnitCost * x.TaxRate / 100m, 2, MidpointRounding.AwayFromZero));
        order.Total = order.Subtotal + order.TaxAmount;
        orders.Update(order);
    }

    public async Task CancelAsync(Guid companyId, Guid purchaseOrderId, CancellationToken cancellationToken = default)
    {
        var order = await orders.Query().FirstOrDefaultAsync(x => x.Id == purchaseOrderId && x.CompanyId == companyId, cancellationToken) ?? throw new InvalidOperationException("Pedido de compra não encontrado.");
        if (order.IsCancelled) return;
        if (order.ReceivedAtUtc is not null) throw new InvalidOperationException("Uma compra já recebida não pode ser cancelada por este fluxo.");
        order.IsCancelled = true;
        orders.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
