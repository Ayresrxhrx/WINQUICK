using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Purchases;

public sealed class PurchaseService(
    IRepository<PurchaseOrder> orders,
    IRepository<PurchaseOrderItem> orderItems,
    IRepository<Product> products,
    IRepository<Supplier> suppliers,
    IRepository<StockBalance> balances,
    IRepository<StockMovement> movements,
    IUnitOfWork unitOfWork) : IPurchaseService
{
    public async Task<PurchaseOrder> CreateAsync(CreatePurchaseCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Items is null || command.Items.Count == 0) throw new ArgumentException("A compra deve possuir pelo menos um item.");
        var supplier = await suppliers.GetByIdAsync(command.SupplierId, cancellationToken)
            ?? throw new KeyNotFoundException("Fornecedor não encontrado.");
        if (supplier.CompanyId != command.CompanyId || !supplier.IsActive) throw new InvalidOperationException("Fornecedor inválido ou inactivo.");

        var order = new PurchaseOrder
        {
            CompanyId = command.CompanyId,
            SupplierId = command.SupplierId,
            UserId = command.UserId,
            Number = $"PO-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..27]
        };

        foreach (var item in command.Items)
        {
            if (item.Quantity <= 0 || item.UnitCost < 0 || item.TaxRate < 0) throw new ArgumentException("Quantidade, custo e IVA inválidos.");
            var product = await products.GetByIdAsync(item.ProductId, cancellationToken)
                ?? throw new KeyNotFoundException("Produto não encontrado.");
            if (product.CompanyId != command.CompanyId || !product.IsActive) throw new InvalidOperationException("Produto inválido ou inactivo.");

            var net = item.Quantity * item.UnitCost;
            var tax = net * item.TaxRate / 100m;
            var line = net + tax;
            order.Subtotal += net;
            order.TaxAmount += tax;
            order.Total += line;

            await orderItems.AddAsync(new PurchaseOrderItem
            {
                PurchaseOrderId = order.Id,
                ProductId = item.ProductId,
                OrderedQuantity = item.Quantity,
                ReceivedQuantity = 0m,
                UnitCost = item.UnitCost,
                TaxRate = item.TaxRate,
                LineTotal = line
            }, cancellationToken);
        }

        await orders.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<PurchaseOrder> ReceiveAsync(ReceivePurchaseCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Items is null || command.Items.Count == 0) throw new ArgumentException("Indique os itens recebidos.");
        var order = await orders.GetByIdAsync(command.PurchaseOrderId, cancellationToken)
            ?? throw new KeyNotFoundException("Pedido de compra não encontrado.");
        if (order.CompanyId != command.CompanyId || order.IsCancelled) throw new InvalidOperationException("Pedido de compra inválido.");

        foreach (var request in command.Items)
        {
            if (request.Quantity <= 0) throw new ArgumentException("A quantidade recebida deve ser maior que zero.");
            var item = await orderItems.GetByIdAsync(request.PurchaseOrderItemId, cancellationToken)
                ?? throw new KeyNotFoundException("Item da compra não encontrado.");
            if (item.PurchaseOrderId != order.Id) throw new InvalidOperationException("O item não pertence ao pedido.");
            var remaining = item.OrderedQuantity - item.ReceivedQuantity;
            if (request.Quantity > remaining) throw new InvalidOperationException("A recepção excede a quantidade pendente.");

            var balance = balances.Query().FirstOrDefault(x => x.CompanyId == command.CompanyId && x.ProductId == item.ProductId);
            if (balance is null)
            {
                balance = new StockBalance { CompanyId = command.CompanyId, ProductId = item.ProductId, Quantity = request.Quantity, ReservedQuantity = 0m, UpdatedAtUtc = DateTime.UtcNow };
                await balances.AddAsync(balance, cancellationToken);
            }
            else
            {
                balance.Quantity += request.Quantity;
                balance.UpdatedAtUtc = DateTime.UtcNow;
                balances.Update(balance);
            }

            item.ReceivedQuantity += request.Quantity;
            orderItems.Update(item);
            await movements.AddAsync(new StockMovement
            {
                CompanyId = command.CompanyId,
                ProductId = item.ProductId,
                TerminalId = command.TerminalId,
                UserId = command.UserId,
                Quantity = request.Quantity,
                UnitCost = item.UnitCost,
                Type = "PURCHASE_RECEIPT",
                Reference = order.Number
            }, cancellationToken);
        }

        var allItems = orders.Query().Where(x => x.Id == order.Id).SelectMany(_ => orderItems.Query()).Where(x => x.PurchaseOrderId == order.Id).ToList();
        if (allItems.All(x => x.ReceivedQuantity >= x.OrderedQuantity)) order.ReceivedAtUtc = DateTime.UtcNow;
        orders.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return order;
    }
}
