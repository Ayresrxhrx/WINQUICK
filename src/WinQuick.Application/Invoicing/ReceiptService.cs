using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Invoicing;

public sealed record IssueReceiptCommand(Guid CompanyId, Guid InvoiceId, decimal Amount, string PaymentMethod, string? Reference, Guid UserId);

public sealed class ReceiptService(
    IRepository<Invoice> invoices,
    IRepository<InvoicePayment> payments,
    IRepository<Receipt> receipts,
    IUnitOfWork unitOfWork)
{
    public async Task<Receipt> IssueAsync(IssueReceiptCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Amount <= 0m) throw new ArgumentException("O valor do recibo deve ser superior a zero.");
        if (string.IsNullOrWhiteSpace(command.PaymentMethod)) throw new ArgumentException("O método de pagamento é obrigatório.");

        var invoice = await invoices.Query().FirstOrDefaultAsync(x => x.Id == command.InvoiceId && x.CompanyId == command.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Factura não encontrada.");
        if (invoice.IsCancelled) throw new InvalidOperationException("Não é possível emitir recibo para uma factura cancelada.");

        var paid = await payments.Query()
            .Where(x => x.CompanyId == command.CompanyId && x.InvoiceId == invoice.Id && !x.IsCancelled)
            .SumAsync(x => x.Amount, cancellationToken);
        var receipted = await receipts.Query()
            .Where(x => x.CompanyId == command.CompanyId && x.InvoiceId == invoice.Id && !x.IsCancelled)
            .SumAsync(x => x.Amount, cancellationToken);
        var available = Math.Max(0m, paid - receipted);
        if (command.Amount > available)
            throw new InvalidOperationException("O valor do recibo não pode exceder o valor efectivamente pago e ainda não documentado.");

        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            CompanyId = command.CompanyId,
            InvoiceId = invoice.Id,
            Amount = command.Amount,
            PaymentMethod = command.PaymentMethod.Trim(),
            Reference = command.Reference?.Trim(),
            UserId = command.UserId,
            IssuedAtUtc = DateTime.UtcNow,
            IsCancelled = false
        };

        await receipts.AddAsync(receipt, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return receipt;
    }
}
