using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;
using WinQuick.Infrastructure.Persistence;

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

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task IncreaseAsync(
        Guid companyId,
        Guid productId,
        decimal quantity,
        Guid userId,
        Guid? terminalId,
        string reference,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade deve ser superior a zero.");

        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("A referência do movimento é obrigatória.", nameof(reference));

        var product = await db.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == productId && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Produto não encontrado para a empresa indicada.");

        if (!product.TrackStock)
            return;

        var balance = await db.StockBalances
            .SingleOrDefaultAsync(x => x.CompanyId == companyId && x.ProductId == productId, cancellationToken);

        if (balance is null)
        {
            balance = new StockBalance
            {
                CompanyId = companyId,
                ProductId = productId,
                Quantity = 0m,
                ReservedQuantity = 0m,
                UpdatedAtUtc = DateTime.UtcNow
            };
            db.StockBalances.Add(balance);
        }

        balance.Quantity += quantity;
        balance.UpdatedAtUtc = DateTime.UtcNow;

        db.StockMovements.Add(new StockMovement
        {
            CompanyId = companyId,
            ProductId = productId,
            TerminalId = terminalId,
            UserId = userId,
            Quantity = quantity,
            UnitCost = product.CostPrice,
            Type = "INCREASE",
            Reference = reference.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AdjustAsync(
        Guid companyId,
        Guid productId,
        decimal targetQuantity,
        Guid userId,
        Guid? terminalId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (targetQuantity < 0m)
            throw new ArgumentOutOfRangeException(nameof(targetQuantity), "A quantidade de stock não pode ser negativa.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("O motivo do ajuste é obrigatório.", nameof(reason));

        var product = await db.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == productId && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Produto não encontrado para a empresa indicada.");

        if (!product.TrackStock)
            return;

        var balance = await db.StockBalances
            .SingleOrDefaultAsync(x => x.CompanyId == companyId && x.ProductId == productId, cancellationToken);

        if (balance is null)
        {
            balance = new StockBalance
            {
                CompanyId = companyId,
                ProductId = productId,
                Quantity = 0m,
                ReservedQuantity = 0m,
                UpdatedAtUtc = DateTime.UtcNow
            };
            db.StockBalances.Add(balance);
        }

        var difference = targetQuantity - balance.Quantity;
        if (difference == 0m)
            return;

        balance.Quantity = targetQuantity;
        balance.UpdatedAtUtc = DateTime.UtcNow;

        db.StockMovements.Add(new StockMovement
        {
            CompanyId = companyId,
            ProductId = productId,
            TerminalId = terminalId,
            UserId = userId,
            Quantity = difference,
            UnitCost = product.CostPrice,
            Type = "ADJUSTMENT",
            Reference = reason.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
