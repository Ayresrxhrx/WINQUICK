using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WinQuick.Core.Entities;

namespace WinQuick.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Number).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Subtotal).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.Total).HasPrecision(18, 4);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 4);
        builder.HasIndex(x => new { x.CompanyId, x.Number }).IsUnique();
        builder.HasIndex(x => new { x.SupplierId, x.CreatedAtUtc });
    }
}
