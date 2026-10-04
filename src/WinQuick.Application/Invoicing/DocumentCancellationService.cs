using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Invoicing;

public sealed class DocumentCancellationService(
    IRepository<Invoice> invoices,
    IRepository<InvoicePayment> invoicePayments,
    IRepository<Receipt> receipts,
    IRepository<CreditNote> creditNotes,
    IUnitOfWork unitOfWork)
{
    public async Task CancelInvoiceAsync(Guid companyId, Guid invoiceId, string reason, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("O motivo do cancelamento é obrigatório.");
        var invoice = await invoices.Query().FirstOrDefaultAsync(x => x.Id == invoiceId && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Factura não encontrada.");
        if (invoice.IsCancelled) throw new InvalidOperationException("A factura já está cancelada.");

        var hasPayments = await invoicePayments.Query().AnyAsync(x => x.CompanyId == companyId && x.InvoiceId == invoiceId && !x.IsCancelled, cancellationToken);
        if (hasPayments) throw new InvalidOperationException("Uma factura com pagamentos registados deve ser regularizada antes do cancelamento.");

        invoice.IsCancelled = true;
        invoice.CancellationReason = reason.Trim();
        invoice.CancelledAtUtc = DateTime.UtcNow;
        invoice.CancelledByUserId = userId;
        invoices.Update(invoice);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelReceiptAsync(Guid companyId, Guid receiptId, string reason, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("O motivo do cancelamento é obrigatório.");
        var receipt = await receipts.Query().FirstOrDefaultAsync(x => x.Id == receiptId && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Recibo não encontrado.");
        if (receipt.IsCancelled) throw new InvalidOperationException("O recibo já está cancelado.");
        receipt.IsCancelled = true;
        receipt.CancellationReason = reason.Trim();
        receipt.CancelledAtUtc = DateTime.UtcNow;
        receipt.CancelledByUserId = userId;
        receipts.Update(receipt);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelCreditNoteAsync(Guid companyId, Guid creditNoteId, string reason, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("O motivo do cancelamento é obrigatório.");
        var note = await creditNotes.Query().FirstOrDefaultAsync(x => x.Id == creditNoteId && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Nota de crédito não encontrada.");
        if (note.IsCancelled) throw new InvalidOperationException("A nota de crédito já está cancelada.");
        note.IsCancelled = true;
        note.CancellationReason = reason.Trim();
        note.CancelledAtUtc = DateTime.UtcNow;
        note.CancelledByUserId = userId;
        creditNotes.Update(note);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
