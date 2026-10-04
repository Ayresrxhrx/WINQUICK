using Microsoft.EntityFrameworkCore;
using WinQuick.Application.Abstractions;
using WinQuick.Core.Entities;

namespace WinQuick.Application.Products;

public sealed class ProductBarcodeService(IRepository<ProductBarcode> barcodes, IRepository<Product> products, IUnitOfWork unitOfWork)
{
    public async Task<ProductBarcode> AddAsync(Guid companyId, Guid productId, string barcode, bool isPrimary = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode)) throw new ArgumentException("O código de barras é obrigatório.");
        var product = await products.Query().FirstOrDefaultAsync(x => x.Id == productId && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Produto não encontrado.");
        var value = barcode.Trim();
        if (await barcodes.Query().AnyAsync(x => x.CompanyId == companyId && x.Barcode == value, cancellationToken))
            throw new InvalidOperationException("Este código de barras já está associado a um produto.");

        if (isPrimary)
        {
            foreach (var current in barcodes.Query().Where(x => x.ProductId == product.Id && x.IsPrimary))
            {
                current.IsPrimary = false;
                barcodes.Update(current);
            }
        }

        var entity = new ProductBarcode { CompanyId = companyId, ProductId = product.Id, Barcode = value, IsPrimary = isPrimary };
        await barcodes.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task RemoveAsync(Guid companyId, Guid barcodeId, CancellationToken cancellationToken = default)
    {
        var entity = await barcodes.Query().FirstOrDefaultAsync(x => x.Id == barcodeId && x.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Código de barras não encontrado.");
        if (entity.IsPrimary && await barcodes.Query().CountAsync(x => x.ProductId == entity.ProductId, cancellationToken) <= 1)
            throw new InvalidOperationException("O produto precisa manter pelo menos um código de barras.");
        barcodes.Delete(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<ProductBarcode?> FindAsync(Guid companyId, string barcode, CancellationToken cancellationToken = default)
        => barcodes.Query().FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Barcode == barcode.Trim(), cancellationToken);
}
