using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Sales;

public sealed class SaleService(
    IRepository<Sale> sales,
    IRepository<Product> products,
    IUnitOfWork unitOfWork) : ISaleService
{
    public async Task<CreateSaleResult> CreateAsync(
        CreateSaleCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateCommand(command);

        var productIds = command.Items.Select(x => x.ProductId).Distinct().ToArray();
        var productList = products.Query()
            .Where(x => productIds.Contains(x.Id) && x.CompanyId == command.CompanyId && x.IsActive)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        if (productList.Count != productIds.Length)
            throw new SaleValidationException("Um ou mais produtos não existem, estão inactivos ou não pertencem à empresa.");

        var productMap = productList.ToDictionary(x => x.Id);
        var subtotal = 0m;
        var discount = 0m;

        foreach (var item in command.Items)
        {
            var product = productMap[item.ProductId];
            var lineSubtotal = item.Quantity * item.UnitPrice;
            if (item.UnitPrice < 0m || item.DiscountAmount < 0m || item.DiscountAmount > lineSubtotal)
                throw new SaleValidationException($"Valores inválidos para o produto {product.Name}.");

            subtotal += lineSubtotal;
            discount += item.DiscountAmount;
        }

        var total = subtotal - discount;
        var applied = command.Payments.Sum(x => x.AmountApplied);
        var tendered = command.Payments.Sum(x => x.AmountTendered);

        if (applied < total)
            throw new SaleValidationException("O valor pago é inferior ao total da venda.");

        var change = Math.Max(0m, tendered - applied);
        var now = DateTime.UtcNow;
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..6];

        var sale = new Sale
        {
            CompanyId = command.CompanyId,
            TerminalId = command.TerminalId,
            UserId = command.UserId,
            CustomerId = command.CustomerId,
            Number = $"V-{now:yyyyMMddHHmmssfff}-{uniqueSuffix}",
            Subtotal = subtotal,
            DiscountAmount = discount,
            TaxAmount = 0m,
            Total = total,
            PaidAmount = applied,
            ChangeAmount = change,
            CreatedAtUtc = now,
            CompletedAtUtc = now
        };

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await sales.AddAsync(sale, ct);
        }, cancellationToken);

        return new CreateSaleResult(
            sale.Id,
            sale.Number,
            sale.Subtotal,
            sale.DiscountAmount,
            sale.TaxAmount,
            sale.Total,
            sale.PaidAmount,
            sale.ChangeAmount);
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
