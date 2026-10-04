using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WinQuick.Core.Entities;

namespace WinQuick.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Sku).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CostPrice).HasPrecision(18, 4);
        builder.Property(x => x.SalePrice).HasPrecision(18, 4);
        builder.Property(x => x.MinimumStock).HasPrecision(18, 4);
        builder.Property(x => x.MaximumStock).HasPrecision(18, 4);
        builder.HasIndex(x => new { x.CompanyId, x.Sku }).IsUnique();
        builder.HasIndex(x => new { x.CompanyId, x.Name });
        builder.HasIndex(x => new { x.CategoryId, x.IsActive });
    }
}
