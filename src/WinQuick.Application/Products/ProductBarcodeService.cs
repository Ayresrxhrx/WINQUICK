using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Products;

public sealed class ProductBarcodeService(IRepository<ProductBarcode> barcodes, IRepository<Product> products, IUnitOfWork unitOfWork)
{
    public async Task<ProductBarcode> AddAsync(Guid companyId, Guid productId, string barcode, bool isPrimary = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode)) throw new ArgumentException("O código de barras é obrigatório.");
        var product = await products.Query().FirstOrDefaultAsync(x => x.Id == productId && x.CompanyId == companyId, cancellationToken) ?? throw new InvalidOperationException("Produto não encontrado.");
        var value = barcode.Trim();
        var existingProductIds = products.Query().Where(x => x.CompanyId == companyId).Select(x => x.Id);
        if (await barcodes.Query().AnyAsync(x => existingProductIds.Contains(x.ProductId) && x.Barcode == value && x.IsActive, cancellationToken)) throw new InvalidOperationException("Este código de barras já está associado a um produto.");
        if (isPrimary)
        {
            foreach (var current in await barcodes.Query().Where(x => x.ProductId == product.Id && x.IsPrimary && x.IsActive).ToListAsync(cancellationToken)) { current.IsPrimary = false; barcodes.Update(current); }
        }
        var entity = new ProductBarcode { ProductId = product.Id, Barcode = value, IsPrimary = isPrimary, IsActive = true };
        await barcodes.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task RemoveAsync(Guid companyId, Guid barcodeId, CancellationToken cancellationToken = default)
    {
        var entity = await barcodes.Query().FirstOrDefaultAsync(x => x.Id == barcodeId && x.IsActive && products.Query().Where(p => p.CompanyId == companyId).Select(p => p.Id).Contains(x.ProductId), cancellationToken) ?? throw new InvalidOperationException("Código de barras não encontrado.");
        entity.IsActive = false;
        entity.IsPrimary = false;
        barcodes.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<ProductBarcode?> FindAsync(Guid companyId, string barcode, CancellationToken cancellationToken = default)
    {
        var productIds = products.Query().Where(x => x.CompanyId == companyId).Select(x => x.Id);
        return barcodes.Query().FirstOrDefaultAsync(x => productIds.Contains(x.ProductId) && x.Barcode == barcode.Trim() && x.IsActive, cancellationToken);
    }
}
