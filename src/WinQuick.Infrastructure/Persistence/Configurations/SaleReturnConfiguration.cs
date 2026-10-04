using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WinQuick.Core.Entities;

namespace WinQuick.Infrastructure.Persistence.Configurations;

public sealed class SaleReturnConfiguration : IEntityTypeConfiguration<SaleReturn>
{
    public void Configure(EntityTypeBuilder<SaleReturn> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Number).HasMaxLength(50).IsRequired();
        b.Property(x => x.TotalAmount).HasPrecision(18, 4);
        b.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        b.HasIndex(x => new { x.CompanyId, x.Number }).IsUnique();
        b.HasIndex(x => new { x.CompanyId, x.SaleId });
        b.HasOne<Sale>().WithMany().HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class SaleReturnItemConfiguration : IEntityTypeConfiguration<SaleReturnItem>
{
    public void Configure(EntityTypeBuilder<SaleReturnItem> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.UnitPrice).HasPrecision(18, 4);
        b.Property(x => x.TaxAmount).HasPrecision(18, 4);
        b.Property(x => x.TotalAmount).HasPrecision(18, 4);
        b.HasIndex(x => new { x.SaleReturnId, x.SaleItemId }).IsUnique();
        b.HasOne<SaleReturn>().WithMany().HasForeignKey(x => x.SaleReturnId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<SaleItem>().WithMany().HasForeignKey(x => x.SaleItemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
