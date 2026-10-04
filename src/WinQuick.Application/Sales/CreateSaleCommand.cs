namespace WinQuick.Application.Sales;

public sealed record CreateSaleCommand(
    Guid CompanyId,
    Guid TerminalId,
    Guid UserId,
    Guid? CustomerId,
    string IdempotencyKey,
    IReadOnlyList<CreateSaleItem> Items,
    IReadOnlyList<CreateSalePayment> Payments);

public sealed record CreateSaleItem(Guid ProductId, decimal Quantity, decimal UnitPrice, decimal DiscountAmount);

public sealed record CreateSalePayment(Guid PaymentMethodId, decimal AmountApplied, decimal AmountTendered, string? Reference);
