using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Products;

public sealed record CreateProductCommand(Guid CompanyId, string Name, string Sku, string? Barcode, Guid? CategoryId, decimal CostPrice, decimal SalePrice, decimal MinimumStock, decimal MaximumStock, Guid? TaxRateId, bool TrackStock = true);
public sealed record UpdateProductCommand(Guid CompanyId, Guid ProductId, string Name, string Sku, string? Barcode, Guid? CategoryId, decimal CostPrice, decimal SalePrice, decimal MinimumStock, decimal MaximumStock, Guid? TaxRateId, bool TrackStock, bool IsActive);

public interface IProductService
{
    Task<Product> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken = default);
    Task<Product> UpdateAsync(UpdateProductCommand command, CancellationToken cancellationToken = default);
    Task<Product?> GetAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchAsync(Guid companyId, string? search, bool includeInactive = false, CancellationToken cancellationToken = default);
}

public sealed class ProductService(IRepository<Product> products, IRepository<ProductBarcode> barcodes, IUnitOfWork unitOfWork) : IProductService
{
    public async Task<Product> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        Validate(command.Name, command.Sku, command.CostPrice, command.SalePrice, command.MinimumStock, command.MaximumStock);
        await EnsureUniqueAsync(command.CompanyId, command.Sku, command.Barcode, null, cancellationToken);
        var product = new Product { CompanyId = command.CompanyId, Name = command.Name.Trim(), Sku = command.Sku.Trim(), CategoryId = command.CategoryId, CostPrice = command.CostPrice, SalePrice = command.SalePrice, MinimumStock = command.MinimumStock, MaximumStock = command.MaximumStock, TaxRateId = command.TaxRateId, TrackStock = command.TrackStock, IsActive = true, CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow };
        await products.AddAsync(product, cancellationToken);
        if (!string.IsNullOrWhiteSpace(command.Barcode)) await barcodes.AddAsync(new ProductBarcode { ProductId = product.Id, Barcode = command.Barcode.Trim(), IsPrimary = true }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product> UpdateAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        Validate(command.Name, command.Sku, command.CostPrice, command.SalePrice, command.MinimumStock, command.MaximumStock);
        var product = await products.Query().FirstOrDefaultAsync(x => x.Id == command.ProductId && x.CompanyId == command.CompanyId, cancellationToken) ?? throw new InvalidOperationException("Produto não encontrado.");
        await EnsureUniqueAsync(command.CompanyId, command.Sku, command.Barcode, product.Id, cancellationToken);
        product.Name = command.Name.Trim(); product.Sku = command.Sku.Trim(); product.CategoryId = command.CategoryId; product.CostPrice = command.CostPrice; product.SalePrice = command.SalePrice; product.MinimumStock = command.MinimumStock; product.MaximumStock = command.MaximumStock; product.TaxRateId = command.TaxRateId; product.TrackStock = command.TrackStock; product.IsActive = command.IsActive; product.UpdatedAtUtc = DateTime.UtcNow;
        products.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return product;
    }

    public Task<Product?> GetAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default) => products.Query().FirstOrDefaultAsync(x => x.Id == productId && x.CompanyId == companyId, cancellationToken);

    public async Task<IReadOnlyList<Product>> SearchAsync(Guid companyId, string? search, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = products.Query().Where(x => x.CompanyId == companyId);
        if (!includeInactive) query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim(); query = query.Where(x => x.Name.Contains(term) || x.Sku.Contains(term)); }
        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    private async Task EnsureUniqueAsync(Guid companyId, string sku, string? barcode, Guid? currentProductId, CancellationToken cancellationToken)
    {
        if (await products.Query().AnyAsync(x => x.CompanyId == companyId && x.Sku == sku.Trim() && x.Id != currentProductId, cancellationToken)) throw new InvalidOperationException("O SKU já está associado a outro produto.");
        if (!string.IsNullOrWhiteSpace(barcode))
        {
            var productIds = products.Query().Where(x => x.CompanyId == companyId && x.Id != currentProductId).Select(x => x.Id);
            if (await barcodes.Query().AnyAsync(x => productIds.Contains(x.ProductId) && x.Barcode == barcode.Trim() && x.IsActive, cancellationToken)) throw new InvalidOperationException("O código de barras já está associado a outro produto.");
        }
    }

    private static void Validate(string name, string sku, decimal cost, decimal price, decimal minimum, decimal maximum)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("O nome do produto é obrigatório.");
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("O SKU é obrigatório.");
        if (cost < 0m || price < 0m) throw new ArgumentException("Os preços não podem ser negativos.");
        if (minimum < 0m || maximum < 0m || maximum < minimum) throw new ArgumentException("Os limites de stock são inválidos.");
    }
}
