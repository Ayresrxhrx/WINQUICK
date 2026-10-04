using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Customers;

public sealed record CreateCustomerCommand(
    Guid CompanyId,
    string Name,
    string? Nuit,
    string? Phone,
    string? Email,
    string? Address,
    decimal CreditLimit);

public sealed record UpdateCustomerCommand(
    Guid CompanyId,
    Guid CustomerId,
    string Name,
    string? Nuit,
    string? Phone,
    string? Email,
    string? Address,
    decimal CreditLimit,
    bool IsActive);

public sealed class CustomerService(IRepository<Customer> customers, IUnitOfWork unitOfWork)
{
    public async Task<Customer> CreateAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        Validate(command.Name, command.CreditLimit);
        var customer = new Customer
        {
            CompanyId = command.CompanyId,
            Name = command.Name.Trim(),
            Nuit = Clean(command.Nuit),
            Phone = Clean(command.Phone),
            Email = Clean(command.Email),
            Address = Clean(command.Address),
            CreditLimit = command.CreditLimit,
            CreditBalance = 0m,
            IsActive = true
        };

        await customers.AddAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task<Customer> UpdateAsync(UpdateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        Validate(command.Name, command.CreditLimit);
        var customer = customers.Query().FirstOrDefault(x => x.Id == command.CustomerId && x.CompanyId == command.CompanyId)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        if (command.CreditLimit < customer.CreditBalance)
            throw new InvalidOperationException("O limite de crédito não pode ser inferior ao saldo de crédito actual.");

        customer.Name = command.Name.Trim();
        customer.Nuit = Clean(command.Nuit);
        customer.Phone = Clean(command.Phone);
        customer.Email = Clean(command.Email);
        customer.Address = Clean(command.Address);
        customer.CreditLimit = command.CreditLimit;
        customer.IsActive = command.IsActive;
        customer.UpdatedAtUtc = DateTime.UtcNow;

        customers.Update(customer);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public IQueryable<Customer> Query(Guid companyId) =>
        customers.Query().Where(x => x.CompanyId == companyId);

    private static void Validate(string name, decimal creditLimit)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("O nome do cliente é obrigatório.");
        if (creditLimit < 0m)
            throw new InvalidOperationException("O limite de crédito não pode ser negativo.");
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
