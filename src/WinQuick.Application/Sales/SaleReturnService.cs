using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Sales;

public sealed record ReturnSaleItemCommand(Guid SaleItemId, decimal Quantity);
public sealed record ReturnSaleCommand(Guid CompanyId, Guid TerminalId, Guid UserId, Guid SaleId, string Reason, IReadOnlyCollection<ReturnSaleItemCommand> Items);

public interface ISaleReturnService
{
    Task<SaleReturn> ReturnAsync(ReturnSaleCommand command, CancellationToken cancellationToken = default);
}

public sealed class SaleReturnService(
    IRepository<Sale> sales,
    IRepository<SaleItem> saleItems,
    IRepository<SaleReturn> returns,
    IRepository<SaleReturnItem> returnItems,
    IRepository<Product> products,
    IRepository<StockBalance> stockBalances,
    IRepository<StockMovement> stockMovements,
    IUnitOfWork unitOfWork) : ISaleReturnService
{
    public async Task<SaleReturn> ReturnAsync(ReturnSaleCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Reason)) throw new ArgumentException("O motivo da devolução é obrigatório.");
        if (command.Items.Count == 0) throw new ArgumentException("A devolução deve possuir pelo menos um item.");

        var sale = sales.Query().FirstOrDefault(x => x.Id == command.SaleId && x.CompanyId == command.CompanyId)
            ?? throw new InvalidOperationException("Venda não encontrada.");

        var requested = command.Items.GroupBy(x => x.SaleItemId).ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
        var saleLineIds = requested.Keys.ToList();
        var lines = saleItems.Query().Where(x => saleLineIds.Contains(x.Id) && x.SaleId == sale.Id).ToList();
        if (lines.Count != requested.Count) throw new InvalidOperationException("Um ou mais itens não pertencem à venda.");

        var previousReturned = returnItems.Query()
            .Join(returns.Query().Where(x => !x.IsCancelled && x.SaleId == sale.Id), i => i.SaleReturnId, r => r.Id, (i, r) => i)
            .Where(i => saleLineIds.Contains(i.SaleItemId))
            .GroupBy(i => i.SaleItemId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        decimal total = 0m;
        var resultItems = new List<(SaleItem Line, decimal Quantity, decimal Total)>();
        foreach (var line in lines)
        {
            var quantity = requested[line.Id];
            var alreadyReturned = previousReturned.GetValueOrDefault(line.Id);
            if (quantity <= 0m || alreadyReturned + quantity > line.Quantity)
                throw new InvalidOperationException("A quantidade devolvida excede a quantidade vendida disponível.");

            var lineTotal = line.UnitPrice * quantity;
            total += lineTotal;
            resultItems.Add((line, quantity, lineTotal));
        }

        var number = $"DEV-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}";
        var saleReturn = new SaleReturn
        {
            CompanyId = command.CompanyId,
            TerminalId = command.TerminalId,
            UserId = command.UserId,
            SaleId = sale.Id,
            Number = number,
            TotalAmount = total,
            Reason = command.Reason.Trim()
        };
        await returns.AddAsync(saleReturn, cancellationToken);

        foreach (var item in resultItems)
        {
            await returnItems.AddAsync(new SaleReturnItem
            {
                SaleReturnId = saleReturn.Id,
                SaleItemId = item.Line.Id,
                ProductId = item.Line.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Line.UnitPrice,
                TaxAmount = 0m,
                TotalAmount = item.Total
            }, cancellationToken);

            var balance = stockBalances.Query().FirstOrDefault(x => x.CompanyId == command.CompanyId && x.ProductId == item.Line.ProductId)
                ?? throw new InvalidOperationException("Saldo de stock do produto não encontrado.");

            balance.Quantity += item.Quantity;
            stockBalances.Update(balance);

            await stockMovements.AddAsync(new StockMovement
            {
                CompanyId = command.CompanyId,
                ProductId = item.Line.ProductId,
                TerminalId = command.TerminalId,
                UserId = command.UserId,
                Quantity = item.Quantity,
                UnitCost = 0m,
                Type = "SALE_RETURN",
                Reference = saleReturn.Number
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return saleReturn;
    }
}
