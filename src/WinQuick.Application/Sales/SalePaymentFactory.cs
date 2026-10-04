using WinQuick.Core.Entities;

namespace WinQuick.Application.Sales;

internal static class SalePaymentFactory
{
    public static IEnumerable<Payment> Create(
        CreateSaleCommand command,
        Guid saleId,
        DateTime paidAtUtc)
    {
        foreach (var payment in command.Payments)
        {
            var change = Math.Max(0m, payment.AmountTendered - payment.AmountApplied);
            yield return new Payment
            {
                CompanyId = command.CompanyId,
                TerminalId = command.TerminalId,
                UserId = command.UserId,
                SaleId = saleId,
                PaymentMethodId = payment.PaymentMethodId,
                AmountApplied = payment.AmountApplied,
                AmountTendered = payment.AmountTendered,
                ChangeAmount = change,
                Reference = string.IsNullOrWhiteSpace(payment.Reference) ? null : payment.Reference.Trim(),
                PaidAtUtc = paidAtUtc,
                IsRefund = false,
                IsCancelled = false
            };
        }
    }
}
