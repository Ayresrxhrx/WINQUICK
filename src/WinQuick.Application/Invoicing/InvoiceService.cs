using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Invoicing;

public sealed record IssueInvoiceCommand(
    Guid CompanyId,
    Guid TerminalId,
    Guid UserId,
    Guid SaleId,
    Guid InvoiceSeriesId,
    CancellationToken CancellationToken = default);

public sealed class InvoiceService(
    IRepository<Invoice> invoices,
    IRepository<InvoiceItem> invoiceItems,
    IRepository<InvoiceSeries> seriesRepository,
    IRepository<Sale> sales,
    IRepository<SaleItem> saleItems,
    IRepository<Product> _products,
    IUnitOfWork unitOfWork)
{
    public async Task<Invoice> IssueFromSaleAsync(
        IssueInvoiceCommand command,
        CancellationToken cancellationToken = default)
    {
        var sale = sales.Query().FirstOrDefault(x =>
            x.Id == command.SaleId &&
            x.CompanyId == command.CompanyId &&
            x.TerminalId == command.TerminalId &&
            !x.IsCancelled);

        if (sale is null)
            throw new InvalidOperationException("Venda não encontrada, cancelada ou incompatível com o terminal.");

        if (invoices.Query().Any(x => x.SaleId == sale.Id && !x.IsCancelled))
            throw new InvalidOperationException("Esta venda já possui uma factura activa.");

        var series = seriesRepository.Query().FirstOrDefault(x =>
            x.Id == command.InvoiceSeriesId &&
            x.CompanyId == command.CompanyId &&
            x.IsActive);

        if (series is null)
            throw new InvalidOperationException("Série de facturação inválida ou inactiva.");

        if (series.NextNumber <= 0)
            throw new InvalidOperationException("A próxima numeração da série é inválida.");

        var number = series.NextNumber.ToString().PadLeft(Math.Max(1, series.NumberLength), '0');
        var items = saleItems.Query().Where(x => x.SaleId == sale.Id).ToList();
        if (items.Count == 0)
            throw new InvalidOperationException("A venda não possui itens para facturar.");

        var invoice = new Invoice
        {
            CompanyId = command.CompanyId,
            TerminalId = command.TerminalId,
            UserId = command.UserId,
            CustomerId = sale.CustomerId,
            SaleId = sale.Id,
            InvoiceSeriesId = series.Id,
            Number = $"{series.Code}/{number}",
            Subtotal = sale.Subtotal,
            DiscountAmount = sale.DiscountAmount,
            TaxAmount = sale.TaxAmount,
            Total = sale.Total,
            PaidAmount = sale.PaidAmount,
            CurrencyCode = "MZN",
            IssuedAtUtc = DateTime.UtcNow
        };

        await invoices.AddAsync(invoice, cancellationToken);

        foreach (var item in items)
        {
            await invoiceItems.AddAsync(new InvoiceItem
            {
                InvoiceId = invoice.Id,
                ProductId = item.ProductId,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                LineTotal = item.LineTotal
            }, cancellationToken);
        }

        series.NextNumber++;
        seriesRepository.Update(series);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return invoice;
    }
}
