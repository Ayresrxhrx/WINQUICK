using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Customers;

public sealed record CustomerCreditSummary(decimal CreditLimit, decimal OutstandingBalance, decimal AvailableCredit);

public sealed class CustomerCreditService(
    IRepository<Customer> customers,
    IRepository<CustomerPayment> payments,
    IUnitOfWork unitOfWork)
{
    public async Task<CustomerCreditSummary> GetSummaryAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await customers.Query()
            .FirstOrDefaultAsync(x => x.Id == customerId && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        var outstanding = await payments.Query()
            .Where(x => x.CompanyId == companyId && x.CustomerId == customerId)
            .SumAsync(x => x.Amount, cancellationToken);

        var available = Math.Max(0m, customer.CreditLimit - outstanding);
        return new CustomerCreditSummary(customer.CreditLimit, outstanding, available);
    }

    public async Task<CustomerPayment> RegisterPaymentAsync(
        Guid companyId,
        Guid customerId,
        decimal amount,
        string reference,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0m)
            throw new ArgumentException("O valor do pagamento deve ser superior a zero.");
        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("A referência do pagamento é obrigatória.");

        var customer = await customers.Query()
            .FirstOrDefaultAsync(x => x.Id == customerId && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        var payment = new CustomerPayment
        {
            CompanyId = companyId,
            CustomerId = customer.Id,
            Amount = amount,
            Reference = reference.Trim(),
            UserId = userId,
            PaidAtUtc = DateTime.UtcNow
        };

        await payments.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return payment;
    }
}
