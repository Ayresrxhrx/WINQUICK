using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Invoicing;

public sealed record CreateCreditNoteCommand(Guid CompanyId, Guid InvoiceId, decimal Amount, string Reason, Guid UserId);

public sealed class CreditNoteService(
    IRepository<Invoice> invoices,
    IRepository<CreditNote> notes,
    IUnitOfWork unitOfWork)
{
    public async Task<CreditNote> CreateAsync(CreateCreditNoteCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Amount <= 0m) throw new ArgumentException("O valor da nota de crédito deve ser superior a zero.");
        if (string.IsNullOrWhiteSpace(command.Reason)) throw new ArgumentException("O motivo da nota de crédito é obrigatório.");

        var invoice = await invoices.Query().FirstOrDefaultAsync(x => x.Id == command.InvoiceId && x.CompanyId == command.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Factura não encontrada.");
        if (invoice.IsCancelled) throw new InvalidOperationException("Não é possível creditar uma factura cancelada.");

        var credited = await notes.Query()
            .Where(x => x.CompanyId == command.CompanyId && x.InvoiceId == invoice.Id && !x.IsCancelled)
            .SumAsync(x => x.Amount, cancellationToken);
        if (credited + command.Amount > invoice.TotalAmount)
            throw new InvalidOperationException("O total das notas de crédito não pode exceder o valor da factura.");

        var note = new CreditNote
        {
            Id = Guid.NewGuid(),
            CompanyId = command.CompanyId,
            InvoiceId = invoice.Id,
            Amount = command.Amount,
            Reason = command.Reason.Trim(),
            UserId = command.UserId,
            CreatedAtUtc = DateTime.UtcNow,
            IsCancelled = false
        };

        await notes.AddAsync(note, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return note;
    }
}
