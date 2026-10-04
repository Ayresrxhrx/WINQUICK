using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Sales;

public sealed record IssueReturnCreditNoteCommand(Guid CompanyId, Guid TerminalId, Guid UserId, Guid SaleReturnId, Guid InvoiceSeriesId, string Reason);

public interface ISaleReturnCreditNoteService
{
    Task<CreditNote> IssueAsync(IssueReturnCreditNoteCommand command, CancellationToken cancellationToken = default);
}

public sealed class SaleReturnCreditNoteService(
    IRepository<SaleReturn> returns,
    IRepository<Sale> sales,
    IRepository<CreditNote> creditNotes,
    IRepository<Invoice> invoices,
    IUnitOfWork unitOfWork) : ISaleReturnCreditNoteService
{
    public async Task<CreditNote> IssueAsync(IssueReturnCreditNoteCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Reason))
            throw new ArgumentException("O motivo da nota de crédito é obrigatório.");

        var saleReturn = returns.Query().FirstOrDefault(x => x.Id == command.SaleReturnId && x.CompanyId == command.CompanyId && !x.IsCancelled)
            ?? throw new InvalidOperationException("Devolução não encontrada ou cancelada.");

        if (creditNotes.Query().Any(x => x.CompanyId == command.CompanyId && x.Reason.Contains(saleReturn.Number)))
            throw new InvalidOperationException("Já existe uma nota de crédito para esta devolução.");

        var sale = sales.Query().FirstOrDefault(x => x.Id == saleReturn.SaleId && x.CompanyId == command.CompanyId)
            ?? throw new InvalidOperationException("Venda original não encontrada.");

        Guid? originalInvoiceId = invoices.Query().Where(x => x.SaleId == sale.Id).Select(x => (Guid?)x.Id).FirstOrDefault();

        var note = new CreditNote
        {
            CompanyId = command.CompanyId,
            TerminalId = command.TerminalId,
            UserId = command.UserId,
            CustomerId = sale.CustomerId,
            OriginalInvoiceId = originalInvoiceId,
            InvoiceSeriesId = command.InvoiceSeriesId,
            Number = $"NC-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}",
            Reason = $"{saleReturn.Number}: {command.Reason.Trim()}",
            TaxAmount = 0m,
            Total = saleReturn.TotalAmount,
            IssuedAtUtc = DateTime.UtcNow
        };

        await creditNotes.AddAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return note;
    }
}
