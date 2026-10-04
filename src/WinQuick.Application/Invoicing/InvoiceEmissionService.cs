using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Invoicing;

public sealed record EmitInvoiceCommand(Guid CompanyId, Guid SaleId, string SeriesCode, Guid UserId);

public sealed class InvoiceEmissionService(
    IRepository<Sale> sales,
    IRepository<SaleItem> saleItems,
    IRepository<Invoice> invoices,
    IRepository<InvoiceItem> invoiceItems,
    InvoiceNumberService numberService,
    IUnitOfWork unitOfWork)
{
    public async Task<Invoice> EmitAsync(EmitInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var sale = await sales.Query().FirstOrDefaultAsync(x => x.Id == command.SaleId && x.CompanyId == command.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Venda não encontrada.");

        if (sale.IsCancelled) throw new InvalidOperationException("Não é possível facturar uma venda cancelada.");

        var existing = await invoices.Query().FirstOrDefaultAsync(x => x.CompanyId == command.CompanyId && x.SaleId == sale.Id && !x.IsCancelled, cancellationToken);
        if (existing is not null) return existing;

        var items = await saleItems.Query().Where(x => x.SaleId == sale.Id && !x.IsCancelled).ToListAsync(cancellationToken);
        if (items.Count == 0) throw new InvalidOperationException("A venda não possui itens facturáveis.");

        var number = await numberService.NextAsync(command.CompanyId, command.SeriesCode, cancellationToken);
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = command.CompanyId,
            SaleId = sale.Id,
            Number = number,
            CustomerId = sale.CustomerId,
            IssueDateUtc = DateTime.UtcNow,
            Subtotal = sale.Subtotal,
            TaxAmount = sale.TaxAmount,
            TotalAmount = sale.TotalAmount,
            UserId = command.UserId,
            IsCancelled = false
        };

        await invoices.AddAsync(invoice, cancellationToken);

        foreach (var item in items)
        {
            await invoiceItems.AddAsync(new InvoiceItem
            {
                Id = Guid.NewGuid(),
                CompanyId = command.CompanyId,
                InvoiceId = invoice.Id,
                ProductId = item.ProductId,
                Description = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return invoice;
    }
}
