using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Suppliers;

public sealed class SupplierService(IRepository<Supplier> suppliers, IUnitOfWork unitOfWork) : ISupplierService
{
    public async Task<Supplier> CreateAsync(CreateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        var name = command.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("O nome do fornecedor é obrigatório.");

        var supplier = new Supplier
        {
            CompanyId = command.CompanyId,
            Name = name,
            Nuit = Normalize(command.Nuit),
            Phone = Normalize(command.Phone),
            Email = Normalize(command.Email),
            Address = Normalize(command.Address)
        };
        await suppliers.AddAsync(supplier, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return supplier;
    }

    public async Task UpdateAsync(UpdateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        var supplier = await suppliers.GetByIdAsync(command.SupplierId, cancellationToken)
            ?? throw new KeyNotFoundException("Fornecedor não encontrado.");
        if (supplier.CompanyId != command.CompanyId) throw new InvalidOperationException("Fornecedor não pertence à empresa.");
        if (string.IsNullOrWhiteSpace(command.Name)) throw new ArgumentException("O nome do fornecedor é obrigatório.");

        supplier.Name = command.Name.Trim();
        supplier.Nuit = Normalize(command.Nuit);
        supplier.Phone = Normalize(command.Phone);
        supplier.Email = Normalize(command.Email);
        supplier.Address = Normalize(command.Address);
        supplier.UpdatedAtUtc = DateTime.UtcNow;
        suppliers.Update(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SetActiveAsync(Guid companyId, Guid supplierId, bool active, CancellationToken cancellationToken = default)
    {
        var supplier = await suppliers.GetByIdAsync(supplierId, cancellationToken)
            ?? throw new KeyNotFoundException("Fornecedor não encontrado.");
        if (supplier.CompanyId != companyId) throw new InvalidOperationException("Fornecedor não pertence à empresa.");
        supplier.IsActive = active;
        supplier.UpdatedAtUtc = DateTime.UtcNow;
        suppliers.Update(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
