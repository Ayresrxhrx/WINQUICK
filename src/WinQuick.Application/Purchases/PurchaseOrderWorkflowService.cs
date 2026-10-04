using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Purchases;

public sealed record AddPurchaseItemCommand(Guid CompanyId, Guid PurchaseOrderId, Guid ProductId, decimal Quantity, decimal UnitCost, decimal TaxAmount = 0m);

public sealed class PurchaseOrderWorkflowService(
    IRepository<PurchaseOrder> orders,
    IRepository<PurchaseOrderItem> items,
    IRepository<Product> products,
    IUnitOfWork unitOfWork)
{
    public async Task<PurchaseOrderItem> AddItemAsync(AddPurchaseItemCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Quantity <= 0m) throw new ArgumentException("A quantidade deve ser superior a zero.");
        if (command.UnitCost < 0m || command.TaxAmount < 0m) throw new ArgumentException("Os valores da compra não podem ser negativos.");

        var order = await orders.Query().FirstOrDefaultAsync(x => x.Id == command.PurchaseOrderId && x.CompanyId == command.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Pedido de compra não encontrado.");
        var product = await products.Query().FirstOrDefaultAsync(x => x.Id == command.ProductId && x.CompanyId == command.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Produto não encontrado.");

        if (order.IsCancelled) throw new InvalidOperationException("Não é possível alterar um pedido de compra cancelado.");

        var item = new PurchaseOrderItem
        {
            Id = Guid.NewGuid(),
            CompanyId = command.CompanyId,
            PurchaseOrderId = order.Id,
            ProductId = product.Id,
            Quantity = command.Quantity,
            UnitCost = command.UnitCost,
            TaxAmount = command.TaxAmount,
            TotalAmount = command.Quantity * command.UnitCost + command.TaxAmount
        };

        await items.AddAsync(item, cancellationToken);
        await RecalculateAsync(order.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task RecalculateAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default)
    {
        var order = await orders.Query().FirstOrDefaultAsync(x => x.Id == purchaseOrderId, cancellationToken)
            ?? throw new InvalidOperationException("Pedido de compra não encontrado.");
        var values = await items.Query().Where(x => x.PurchaseOrderId == purchaseOrderId && !x.IsCancelled)
            .GroupBy(x => 1)
            .Select(g => new { Subtotal = g.Sum(x => x.Quantity * x.UnitCost), Tax = g.Sum(x => x.TaxAmount) })
            .FirstOrDefaultAsync(cancellationToken);

        order.Subtotal = values?.Subtotal ?? 0m;
        order.TaxAmount = values?.Tax ?? 0m;
        order.TotalAmount = order.Subtotal + order.TaxAmount;
        orders.Update(order);
    }

    public async Task CancelAsync(Guid companyId, Guid purchaseOrderId, CancellationToken cancellationToken = default)
    {
        var order = await orders.Query().FirstOrDefaultAsync(x => x.Id == purchaseOrderId && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Pedido de compra não encontrado.");
        if (order.IsCancelled) return;
        order.IsCancelled = true;
        order.UpdatedAtUtc = DateTime.UtcNow;
        orders.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
