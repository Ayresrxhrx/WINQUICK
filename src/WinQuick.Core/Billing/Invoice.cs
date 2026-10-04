namespace WinQuick.Core.Billing;

public sealed class Invoice
{
    private readonly List<InvoiceLine> _lines = [];

    private Invoice() { }

    public Guid Id { get; private set; }
    public Guid? SaleId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public InvoiceNumber Number { get; private set; } = null!;
    public DocumentType Type { get; private set; }
    public DocumentStatus Status { get; private set; }
    public DateTimeOffset IssuedAt { get; private set; }
    public string Currency { get; private set; } = "MZN";
    public IReadOnlyCollection<InvoiceLine> Lines => _lines;
    public InvoiceTotals Totals => BillingCalculator.Calculate(_lines);

    public static Invoice Issue(
        InvoiceNumber number,
        DocumentType type,
        Guid? saleId,
        Guid? customerId,
        IEnumerable<InvoiceLine> lines,
        DateTimeOffset issuedAt)
    {
        ArgumentNullException.ThrowIfNull(lines);
        var invoiceLines = lines.ToArray();
        if (invoiceLines.Length == 0)
            throw new InvalidOperationException("O documento deve possuir pelo menos uma linha.");

        foreach (var line in invoiceLines)
        {
            if (line.Quantity <= 0)
                throw new InvalidOperationException("A quantidade deve ser superior a zero.");
            if (line.UnitPrice < 0)
                throw new InvalidOperationException("O preço não pode ser negativo.");
            if (line.Discount < 0 || line.Discount > line.Quantity * line.UnitPrice)
                throw new InvalidOperationException("O desconto da linha é inválido.");
            if (line.VatRate < 0 || line.VatRate > 100)
                throw new InvalidOperationException("A taxa de IVA é inválida.");
        }

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            SaleId = saleId,
            CustomerId = customerId,
            Number = number,
            Type = type,
            Status = DocumentStatus.Issued,
            IssuedAt = issuedAt
        };
        invoice._lines.AddRange(invoiceLines);
        return invoice;
    }

    public void Cancel()
    {
        if (Status != DocumentStatus.Issued)
            throw new InvalidOperationException("Apenas documentos emitidos podem ser anulados.");
        Status = DocumentStatus.Cancelled;
    }
}
