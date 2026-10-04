using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Invoicing;

public enum InvoicePaperFormat { A4, Thermal80mm }

public sealed record InvoiceDocumentOptions(InvoicePaperFormat PaperFormat, string CurrencySymbol = "MT", string? HeaderText = null, string? FooterText = null);
public sealed record InvoiceDocument(Guid InvoiceId, string Number, InvoicePaperFormat PaperFormat, string ContentHtml, DateTime GeneratedAtUtc);

public sealed class InvoiceDocumentService(IRepository<Invoice> invoices, IRepository<InvoiceItem> items, IRepository<Customer> customers, IRepository<Company> companies)
{
    public async Task<InvoiceDocument> GenerateAsync(Guid companyId, Guid invoiceId, InvoiceDocumentOptions options, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.CurrencySymbol)) throw new ArgumentException("O símbolo da moeda é obrigatório.");
        var invoice = await invoices.Query().FirstOrDefaultAsync(x => x.Id == invoiceId && x.CompanyId == companyId, cancellationToken) ?? throw new InvalidOperationException("Factura não encontrada.");
        var company = await companies.Query().FirstOrDefaultAsync(x => x.Id == companyId, cancellationToken) ?? throw new InvalidOperationException("Empresa não encontrada.");
        var customer = invoice.CustomerId.HasValue ? await customers.Query().FirstOrDefaultAsync(x => x.Id == invoice.CustomerId.Value && x.CompanyId == companyId, cancellationToken) : null;
        var lines = await items.Query().Where(x => x.InvoiceId == invoice.Id).OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var html = BuildHtml(invoice, company, customer, lines, options);
        return new InvoiceDocument(invoice.Id, invoice.Number, options.PaperFormat, html, DateTime.UtcNow);
    }

    private static string BuildHtml(Invoice invoice, Company company, Customer? customer, IReadOnlyCollection<InvoiceItem> lines, InvoiceDocumentOptions options)
    {
        var page = options.PaperFormat == InvoicePaperFormat.A4 ? "A4" : "80mm auto";
        var lineRows = string.Join("", lines.Select(x => $"<tr><td>{Escape(x.Description)}</td><td class='qty'>{x.Quantity:N2}</td><td class='money'>{options.CurrencySymbol} {x.UnitPrice:N2}</td><td class='money'>{options.CurrencySymbol} {x.TaxAmount:N2}</td><td class='money'>{options.CurrencySymbol} {x.LineTotal:N2}</td></tr>"));
        var customerBlock = customer is null ? "Consumidor final" : Escape(customer.Name);
        return $"<!doctype html><html><head><meta charset='utf-8'><title>Factura {Escape(invoice.Number)}</title><style>@page{{size:{page};margin:14mm}}body{{font-family:Arial,sans-serif;color:#151515;font-size:12px;margin:0}}.header{{display:flex;justify-content:space-between;border-bottom:2px solid #111;padding-bottom:14px;margin-bottom:18px}}h1{{font-size:22px;margin:0 0 5px}}h2{{font-size:16px;margin:0}}.meta{{text-align:right}}.section{{margin:14px 0}}table{{width:100%;border-collapse:collapse}}th{{background:#f1f3f5;text-align:left;padding:8px}}td{{padding:8px;border-bottom:1px solid #ddd}}.qty{{text-align:center}}.money{{text-align:right;white-space:nowrap}}.totals{{width:45%;margin-left:auto;margin-top:18px}}.totals td{{border:0}}.grand{{font-size:16px;font-weight:700;border-top:2px solid #111!important}}.footer{{margin-top:35px;border-top:1px solid #ddd;padding-top:10px;font-size:10px;color:#555}}</style></head><body><header class='header'><div><h1>{Escape(company.Name)}</h1><div>{Escape(company.LegalName ?? "")}</div><div>{Escape(company.Address ?? "")} · {Escape(company.Phone ?? "")}</div><div>NIF/NUIT: {Escape(company.Nuit ?? "")}</div></div><div class='meta'><h2>FACTURA</h2><strong>{Escape(invoice.Number)}</strong><div>{invoice.IssuedAtUtc:dd/MM/yyyy HH:mm}</div></div></header><section class='section'><strong>Cliente:</strong> {customerBlock}</section><table><thead><tr><th>Descrição</th><th>Qtd.</th><th>Preço</th><th>IVA</th><th>Total</th></tr></thead><tbody>{lineRows}</tbody></table><table class='totals'><tr><td>Subtotal</td><td class='money'>{options.CurrencySymbol} {invoice.Subtotal:N2}</td></tr><tr><td>Desconto</td><td class='money'>{options.CurrencySymbol} {invoice.DiscountAmount:N2}</td></tr><tr><td>IVA</td><td class='money'>{options.CurrencySymbol} {invoice.TaxAmount:N2}</td></tr><tr class='grand'><td>Total</td><td class='money'>{options.CurrencySymbol} {invoice.Total:N2}</td></tr></table><footer class='footer'>{Escape(options.FooterText ?? "Obrigado pela preferência.")}</footer></body></html>";
    }

    private static string Escape(string value) => System.Net.WebUtility.HtmlEncode(value);
}
