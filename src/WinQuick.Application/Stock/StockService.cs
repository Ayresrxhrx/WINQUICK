using WinQuick.Application.Abstractions;
using WinQuick.Application.Security;
using WinQuick.Core.Entities;
using WinQuick.Core.Security;

namespace WinQuick.Application.Stock;

public sealed class StockService(
    IRepository<Product> products,
    IRepository<StockBalance> balances,
    IRepository<StockMovement> movements,
    IPermissionService permissions,
    IUnitOfWork unitOfWork) : IStockService
{
    public async Task DecreaseAsync(Guid companyId, Guid productId, decimal quantity, Guid userId, Guid terminalId, string reference, CancellationToken cancellationToken = default)
    {
        ValidateQuantity(quantity);
        var product = await GetProductAsync(companyId, productId, cancellationToken);
        if (!product.TrackStock) return;

        var balance = await GetBalanceAsync(companyId, productId, cancellationToken);
        if (balance.AvailableQuantity < quantity)
            throw new InvalidOperationException($"Stock insuficiente para o produto '{product.Name}'. Disponível: {balance.AvailableQuantity:0.###}.");

        balance.Quantity -= quantity;
        balance.UpdatedAtUtc = DateTime.UtcNow;
        balances.Update(balance);

        await movements.AddAsync(new StockMovement
        {
            CompanyId = companyId,
            ProductId = productId,
            TerminalId = terminalId,
            UserId = userId,
            Quantity = -quantity,
            UnitCost = product.CostPrice,
            Type = "SALE",
            Reference = reference
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task IncreaseAsync(Guid companyId, Guid productId, decimal quantity, Guid userId, Guid? terminalId, string reference, CancellationToken cancellationToken = default)
    {
        ValidateQuantity(quantity);
        var product = await GetProductAsync(companyId, productId, cancellationToken);
        if (!product.TrackStock) return;

        var balance = balances.Query().FirstOrDefault(x => x.CompanyId == companyId && x.ProductId == productId);
        if (balance is null)
        {
            balance = new StockBalance { CompanyId = companyId, ProductId = productId, Quantity = 0m };
            await balances.AddAsync(balance, cancellationToken);
        }

        balance.Quantity += quantity;
        balance.UpdatedAtUtc = DateTime.UtcNow;
        balances.Update(balance);

        await movements.AddAsync(new StockMovement
        {
            CompanyId = companyId,
            ProductId = productId,
            TerminalId = terminalId,
            UserId = userId,
            Quantity = quantity,
            UnitCost = product.CostPrice,
            Type = "INCREASE",
            Reference = reference
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AdjustAsync(Guid companyId, Guid productId, decimal targetQuantity, Guid userId, Guid? terminalId, string reason, CancellationToken cancellationToken = default)
    {
        if (targetQuantity < 0m) throw new ArgumentException("A quantidade de stock não pode ser negativa.");
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("O motivo do ajuste é obrigatório.");
        await permissions.EnsurePermissionAsync(userId, companyId, Permission.StockAdjust, cancellationToken);

        var product = await GetProductAsync(companyId, productId, cancellationToken);
        if (!product.TrackStock) return;

        var balance = balances.Query().FirstOrDefault(x => x.CompanyId == companyId && x.ProductId == productId);
        if (balance is null)
        {
            balance = new StockBalance { CompanyId = companyId, ProductId = productId, Quantity = 0m };
            await balances.AddAsync(balance, cancellationToken);
        }

        var difference = targetQuantity - balance.Quantity;
        if (difference == 0m) return;

        balance.Quantity = targetQuantity;
        balance.UpdatedAtUtc = DateTime.UtcNow;
        balances.Update(balance);

        await movements.AddAsync(new StockMovement
        {
            CompanyId = companyId,
            ProductId = productId,
            TerminalId = terminalId,
            UserId = userId,
            Quantity = difference,
            UnitCost = product.CostPrice,
            Type = "ADJUSTMENT",
            Reference = reason.Trim()
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Product> GetProductAsync(Guid companyId, Guid productId, CancellationToken cancellationToken)
        => await products.Query().FirstOrDefaultAsync(x => x.Id == productId && x.CompanyId == companyId)
           ?? throw new InvalidOperationException("Produto não encontrado.");

    private async Task<StockBalance> GetBalanceAsync(Guid companyId, Guid productId, CancellationToken cancellationToken)
        => await balances.Query().FirstOrDefaultAsync(x => x.CompanyId == companyId && x.ProductId == productId)
           ?? throw new InvalidOperationException("Saldo de stock não encontrado.");

    private static void ValidateQuantity(decimal quantity)
    {
        if (quantity <= 0m) throw new ArgumentException("A quantidade deve ser maior que zero.");
    }
}
