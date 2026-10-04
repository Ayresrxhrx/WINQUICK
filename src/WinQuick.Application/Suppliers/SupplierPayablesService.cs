using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Suppliers;

public sealed record SupplierPayableSummary(decimal TotalPayable, decimal TotalPaid, decimal Outstanding);

public sealed class SupplierPayablesService(
    IRepository<Supplier> suppliers,
    IRepository<PurchaseOrder> purchaseOrders,
    IRepository<SupplierPayment> payments,
    IUnitOfWork unitOfWork)
{
    public async Task<SupplierPayableSummary> GetSummaryAsync(Guid companyId, Guid supplierId, CancellationToken cancellationToken = default)
    {
        await EnsureSupplierAsync(companyId, supplierId, cancellationToken);

        var totalPayable = await purchaseOrders.Query()
            .Where(x => x.CompanyId == companyId && x.SupplierId == supplierId && !x.IsCancelled)
            .SumAsync(x => x.TotalAmount, cancellationToken);

        var totalPaid = await payments.Query()
            .Where(x => x.CompanyId == companyId && x.SupplierId == supplierId && !x.IsCancelled)
            .SumAsync(x => x.Amount, cancellationToken);

        return new SupplierPayableSummary(totalPayable, totalPaid, Math.Max(0m, totalPayable - totalPaid));
    }

    public async Task<SupplierPayment> RegisterPaymentAsync(Guid companyId, Guid supplierId, decimal amount, string method, string? reference, Guid userId, CancellationToken cancellationToken = default)
    {
        if (amount <= 0m) throw new ArgumentException("O valor do pagamento deve ser superior a zero.");
        if (string.IsNullOrWhiteSpace(method)) throw new ArgumentException("O método de pagamento é obrigatório.");
        await EnsureSupplierAsync(companyId, supplierId, cancellationToken);

        var summary = await GetSummaryAsync(companyId, supplierId, cancellationToken);
        if (amount > summary.Outstanding)
            throw new InvalidOperationException("O pagamento excede o saldo em dívida do fornecedor.");

        var payment = new SupplierPayment
        {
            CompanyId = companyId,
            SupplierId = supplierId,
            Amount = amount,
            PaymentMethod = method.Trim(),
            Reference = reference?.Trim(),
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            IsCancelled = false
        };

        await payments.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return payment;
    }

    private async Task EnsureSupplierAsync(Guid companyId, Guid supplierId, CancellationToken cancellationToken)
    {
        if (!await suppliers.Query().AnyAsync(x => x.Id == supplierId && x.CompanyId == companyId, cancellationToken))
            throw new InvalidOperationException("Fornecedor não encontrado.");
    }
}
