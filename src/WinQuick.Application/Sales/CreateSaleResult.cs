namespace WinQuick.Application.Sales;

public sealed record CreateSaleResult(
    Guid SaleId,
    string SaleNumber,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal Total,
    decimal PaidAmount,
    decimal ChangeAmount);
