using WinQuick.Core.Entities;

namespace WinQuick.Application.Sales;

internal static class SaleItemFactory
{
    public static SaleItem Create(CreateSaleItem commandItem, Product product, Guid saleId)
    {
        var lineSubtotal = commandItem.Quantity * commandItem.UnitPrice;
        var taxableAmount = lineSubtotal - commandItem.DiscountAmount;
        var tax = 0m;
        var total = taxableAmount + tax;

        return new SaleItem
        {
            SaleId = saleId,
            ProductId = product.Id,
            Description = product.Name,
            Quantity = commandItem.Quantity,
            UnitPrice = commandItem.UnitPrice,
            DiscountAmount = commandItem.DiscountAmount,
            TaxRate = 0m,
            TaxAmount = tax,
            LineTotal = total
        };
    }
}
