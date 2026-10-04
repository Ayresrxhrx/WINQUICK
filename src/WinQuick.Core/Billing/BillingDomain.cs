namespace WinQuick.Core.Billing;

public enum DocumentType
{
    Invoice = 1,
    SimplifiedInvoice = 2,
    InvoiceReceipt = 3,
    Receipt = 4,
    CreditNote = 5,
    DebitNote = 6
}

public enum DocumentStatus
{
    Draft = 1,
    Issued = 2,
    Cancelled = 3,
    Reversed = 4
}

public sealed record DocumentSeries(
    Guid Id,
    string Code,
    DocumentType DocumentType,
    int FiscalYear,
    long NextNumber,
    bool Active);

public sealed record InvoiceLine(
    Guid ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal VatRate)
{
    public decimal NetAmount => Math.Round(Math.Max(0m, Quantity * UnitPrice - Discount), 2, MidpointRounding.AwayFromZero);
    public decimal VatAmount => Math.Round(NetAmount * VatRate / 100m, 2, MidpointRounding.AwayFromZero);
    public decimal GrossAmount => NetAmount + VatAmount;
}

public sealed record InvoiceTotals(decimal Net, decimal Vat, decimal Gross, decimal Discount);

public static class BillingCalculator
{
    public static InvoiceTotals Calculate(IEnumerable<InvoiceLine> lines)
    {
        var materialized = lines.ToArray();
        var net = materialized.Sum(x => x.NetAmount);
        var vat = materialized.Sum(x => x.VatAmount);
        var discount = materialized.Sum(x => x.Discount);
        return new InvoiceTotals(
            Math.Round(net, 2, MidpointRounding.AwayFromZero),
            Math.Round(vat, 2, MidpointRounding.AwayFromZero),
            Math.Round(net + vat, 2, MidpointRounding.AwayFromZero),
            Math.Round(discount, 2, MidpointRounding.AwayFromZero));
    }
}

public sealed record InvoiceNumber(string Series, long Number, int FiscalYear)
{
    public override string ToString() => $"{Series}/{FiscalYear}/{Number:000000}";
}
