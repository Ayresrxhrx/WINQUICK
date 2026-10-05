using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WinQuick.Core.Entities;

namespace WinQuick.Infrastructure.Persistence.Configurations;

internal static class EntityConfigurationExtensions
{
    public static void ConfigureTenantEntity<TEntity>(this EntityTypeBuilder<TEntity> builder) where TEntity : class
    {
        builder.Property<Guid>("CompanyId").IsRequired();
    }
}

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Nuit).HasMaxLength(30);
        b.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        b.Property(x => x.CurrencySymbol).HasMaxLength(10).IsRequired();
    }
}

public sealed class TerminalConfiguration : IEntityTypeConfiguration<Terminal>
{
    public void Configure(EntityTypeBuilder<Terminal> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
    }
}

public sealed class ProductBarcodeConfiguration : IEntityTypeConfiguration<ProductBarcode>
{
    public void Configure(EntityTypeBuilder<ProductBarcode> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Barcode).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Barcode).IsUnique();
        b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
    }
}

public sealed class TaxRateConfiguration : IEntityTypeConfiguration<TaxRate>
{
    public void Configure(EntityTypeBuilder<TaxRate> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Rate).HasPrecision(9, 4);
        b.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
    }
}
