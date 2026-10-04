using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Stock;
using WinQuick.Infrastructure.Persistence;
using WinQuick.Core.Entities;

namespace WinQuick.Infrastructure.Stock;

public sealed class StockService(WinQuickDbContext db) : IStockService
{
    public async Task DecreaseAsync(
        Guid companyId,
        Guid productId,
        decimal quantity,
        Guid userId,
        Guid terminalId,
        string reference,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade deve ser superior a zero.");

        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("A referência do movimento é obrigatória.", nameof(reference));

        var affected = await db.StockBalances
            .Where(x => x.CompanyId == companyId
                     && x.ProductId == productId
                     && x.Quantity - x.ReservedQuantity >= quantity)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Quantity, x => x.Quantity - quantity)
                .SetProperty(x => x.UpdatedAtUtc, _ => DateTime.UtcNow), cancellationToken);

        if (affected != 1)
            throw new InvalidOperationException("Stock insuficiente ou saldo de stock inexistente.");

        var product = await db.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == productId && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Produto não encontrado para a empresa indicada.");

        db.StockMovements.Add(new StockMovement
        {
            CompanyId = companyId,
            ProductId = productId,
            TerminalId = terminalId,
            UserId = userId,
            Quantity = -quantity,
            UnitCost = product.CostPrice,
            Type = "SALE",
            Reference = reference.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        });
    }
}
