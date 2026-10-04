using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;
using WinQuick.Infrastructure.Persistence;

namespace WinQuick.Infrastructure.Inventory;

public sealed class StockService(WinQuickDbContext db) : IStockService
{
    public async Task DecreaseAsync(Guid companyId, Guid terminalId, Guid userId, Guid productId, decimal quantity, decimal unitCost, string reference, CancellationToken cancellationToken = default)
    {
        ValidateQuantity(quantity);

        var affected = await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE StockBalances
            SET Quantity = Quantity - {quantity}, UpdatedAtUtc = {DateTime.UtcNow}
            WHERE CompanyId = {companyId}
              AND ProductId = {productId}
              AND Quantity - ReservedQuantity >= {quantity};
            """, cancellationToken);

        if (affected != 1)
            throw new InvalidOperationException("Stock insuficiente ou saldo de stock inexistente para o produto solicitado.");

        db.StockMovements.Add(new StockMovement
        {
            CompanyId = companyId,
            ProductId = productId,
            TerminalId = terminalId,
            UserId = userId,
            Quantity = -quantity,
            UnitCost = unitCost,
            Type = "SALE",
            Reference = reference
        });
    }

    public async Task IncreaseAsync(Guid companyId, Guid terminalId, Guid userId, Guid productId, decimal quantity, decimal unitCost, string reference, CancellationToken cancellationToken = default)
    {
        ValidateQuantity(quantity);

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO StockBalances (Id, CompanyId, ProductId, Quantity, ReservedQuantity, UpdatedAtUtc)
            VALUES ({Guid.NewGuid()}, {companyId}, {productId}, {quantity}, 0, {DateTime.UtcNow})
            ON CONFLICT (CompanyId, ProductId)
            DO UPDATE SET Quantity = StockBalances.Quantity + excluded.Quantity,
                          UpdatedAtUtc = excluded.UpdatedAtUtc;
            """, cancellationToken);

        db.StockMovements.Add(new StockMovement
        {
            CompanyId = companyId,
            ProductId = productId,
            TerminalId = terminalId,
            UserId = userId,
            Quantity = quantity,
            UnitCost = unitCost,
            Type = "PURCHASE",
            Reference = reference
        });
    }

    private static void ValidateQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade deve ser superior a zero.");
    }
}
