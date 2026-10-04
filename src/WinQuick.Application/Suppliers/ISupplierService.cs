using WinQuick.Core.Entities;

namespace WinQuick.Application.Suppliers;

public interface ISupplierService
{
    Task<Supplier> CreateAsync(CreateSupplierCommand command, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateSupplierCommand command, CancellationToken cancellationToken = default);
    Task SetActiveAsync(Guid companyId, Guid supplierId, bool active, CancellationToken cancellationToken = default);
}

public sealed record CreateSupplierCommand(Guid CompanyId, string Name, string? Nuit, string? Phone, string? Email, string? Address);
public sealed record UpdateSupplierCommand(Guid CompanyId, Guid SupplierId, string Name, string? Nuit, string? Phone, string? Email, string? Address);
