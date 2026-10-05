using WinQuick.Application.Abstractions;
using WinQuick.Application.Ingredients;
using WinQuick.Application.Security;
using StockApplicationService = WinQuick.Application.Stock.IStockService;
using WinQuick.Core.Entities;
using WinQuick.Core.Security;

namespace WinQuick.Application.Sales;

public sealed class SaleService(
    IRepository<Sale> sales,
    IRepository<SaleItem> saleItems,
    IRepository<Payment> payments,
    IRepository<Product> products,
    StockApplicationService stock,
    IIngredientService ingredients,
    IPermissionService permissions,
    IUnitOfWork unitOfWork) : ISaleService
{
    public async Task<CreateSaleResult> CreateAsync(CreateSaleCommand command, CancellationToken cancellationToken = default)
    {
        ValidateCommand(command);
        await permissions.EnsurePermissionAsync(command.UserId, command.CompanyId, Permission.SaleCreate, cancellationToken);

        var productIds = command.Items.Select(x => x.ProductId).Distinct().ToArray();
        var productList = products.Query().Where(x => productIds.Contains(x.Id) && x.CompanyId == command.CompanyId && x.IsActive).ToList();
        cancellationToken.ThrowIfCancellationRequested();
        if (productList.Count != productIds.Length) throw new SaleValidationException("Um ou mais produtos não existem, estão inactivos ou não pertencem à empresa.");

        var productMap = productList.ToDictionary(x => x.Id);
        var subtotal = 0m; var discount = 0m; var tax = 0m;
        var hasDiscount = false;
        var hasManualPrice = false;

        foreach (var item in command.Items)
        {
            var product = productMap[item.ProductId];
            var lineSubtotal = item.Quantity * item.UnitPrice;
            if (item.UnitPrice < 0m || item.DiscountAmount < 0m || item.DiscountAmount > lineSubtotal) throw new SaleValidationException($"Valores inválidos para o produto {product.Name}.");
            if (item.DiscountAmount > 0m) hasDiscount = true;
            if (item.UnitPrice != product.SalePrice) hasManualPrice = true;
            var taxableAmount = lineSubtotal - item.DiscountAmount;
            subtotal += lineSubtotal; discount += item.DiscountAmount; tax += taxableAmount * product.TaxRate / 100m;
        }

        if (hasDiscount) await permissions.EnsurePermissionAsync(command.UserId, command.CompanyId, Permission.SaleDiscount, cancellationToken);
        if (hasManualPrice) await permissions.EnsurePermissionAsync(command.UserId, command.CompanyId, Permission.SaleManualPrice, cancellationToken);

        var total = subtotal - discount + tax;
        var applied = command.Payments.Sum(x => x.AmountApplied);
        var tendered = command.Payments.Sum(x => x.AmountTendered);
        if (applied < total) throw new SaleValidationException("O valor pago é inferior ao total da venda.");
        var change = Math.Max(0m, tendered - applied);
        var now = DateTime.UtcNow;
        var sale = new Sale
        {
            Id = Guid.NewGuid(), CompanyId = command.CompanyId, TerminalId = command.TerminalId, UserId = command.UserId,
            CustomerId = command.CustomerId, Number = $"V-{now:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..23],
            Subtotal = subtotal, DiscountAmount = discount, TaxAmount = tax, Total = total,
            PaidAmount = applied, ChangeAmount = change, CreatedAtUtc = now, CompletedAtUtc = now
        };

        var createdItems = command.Items.Select(item => SaleItemFactory.Create(item, productMap[item.ProductId], sale.Id)).ToArray();
        var createdPayments = SalePaymentFactory.Create(command, sale.Id, now).ToArray();

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await sales.AddAsync(sale, ct);
            foreach (var item in createdItems)
            {
                await stock.DecreaseAsync(command.CompanyId, item.ProductId, item.Quantity, command.UserId, command.TerminalId, sale.Number, ct);
                await ingredients.ConsumeForSaleAsync(command.CompanyId, item.ProductId, item.Quantity, command.UserId, command.TerminalId, sale.Number, ct);
                await saleItems.AddAsync(item, ct);
            }
            foreach (var payment in createdPayments) await payments.AddAsync(payment, ct);
        }, cancellationToken);

        return new CreateSaleResult(sale.Id, sale.Number, sale.Subtotal, sale.DiscountAmount, sale.TaxAmount, sale.Total, sale.PaidAmount, sale.ChangeAmount);
    }

    private static void ValidateCommand(CreateSaleCommand command)
    {
        if (command.CompanyId == Guid.Empty) throw new SaleValidationException("Empresa inválida.");
        if (command.TerminalId == Guid.Empty) throw new SaleValidationException("Terminal inválido.");
        if (command.UserId == Guid.Empty) throw new SaleValidationException("Operador inválido.");
        if (string.IsNullOrWhiteSpace(command.IdempotencyKey)) throw new SaleValidationException("A chave de idempotência é obrigatória.");
        if (command.Items.Count == 0) throw new SaleValidationException("A venda deve possuir pelo menos um item.");
        if (command.Items.Any(x => x.ProductId == Guid.Empty || x.Quantity <= 0m)) throw new SaleValidationException("Existem itens de venda inválidos.");
        if (command.Payments.Count == 0) throw new SaleValidationException("A venda deve possuir pelo menos um pagamento.");
        if (command.Payments.Any(x => x.PaymentMethodId == Guid.Empty || x.AmountApplied <= 0m || x.AmountTendered < 0m)) throw new SaleValidationException("Existem pagamentos inválidos.");
    }
}
