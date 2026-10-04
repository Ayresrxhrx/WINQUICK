using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Invoicing;

public sealed record RegisterInvoicePaymentCommand(Guid CompanyId, Guid InvoiceId, decimal Amount, string PaymentMethod, string? Reference, Guid UserId);

public sealed class InvoicePaymentService(
    IRepository<Invoice> invoices,
    IRepository<InvoicePayment> payments,
    IUnitOfWork unitOfWork)
{
    public async Task<InvoicePayment> RegisterAsync(RegisterInvoicePaymentCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Amount <= 0m) throw new ArgumentException("O valor do pagamento deve ser superior a zero.");
        if (string.IsNullOrWhiteSpace(command.PaymentMethod)) throw new ArgumentException("O método de pagamento é obrigatório.");

        var invoice = await invoices.Query().FirstOrDefaultAsync(x => x.Id == command.InvoiceId && x.CompanyId == command.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Factura não encontrada.");
        if (invoice.IsCancelled) throw new InvalidOperationException("Não é possível pagar uma factura cancelada.");

        var paid = await payments.Query()
            .Where(x => x.CompanyId == command.CompanyId && x.InvoiceId == invoice.Id && !x.IsCancelled)
            .SumAsync(x => x.Amount, cancellationToken);
        var outstanding = Math.Max(0m, invoice.TotalAmount - paid);
        if (command.Amount > outstanding) throw new InvalidOperationException("O pagamento excede o saldo da factura.");

        var payment = new InvoicePayment
        {
            Id = Guid.NewGuid(),
            CompanyId = command.CompanyId,
            InvoiceId = invoice.Id,
            Amount = command.Amount,
            PaymentMethod = command.PaymentMethod.Trim(),
            Reference = command.Reference?.Trim(),
            UserId = command.UserId,
            CreatedAtUtc = DateTime.UtcNow,
            IsCancelled = false
        };

        await payments.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<decimal> GetOutstandingAsync(Guid companyId, Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await invoices.Query().FirstOrDefaultAsync(x => x.Id == invoiceId && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Factura não encontrada.");
        var paid = await payments.Query().Where(x => x.CompanyId == companyId && x.InvoiceId == invoiceId && !x.IsCancelled).SumAsync(x => x.Amount, cancellationToken);
        return Math.Max(0m, invoice.TotalAmount - paid);
    }
}
