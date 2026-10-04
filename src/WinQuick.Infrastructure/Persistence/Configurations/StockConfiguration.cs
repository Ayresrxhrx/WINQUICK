using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WinQuick.Core.Entities;

namespace WinQuick.Infrastructure.Persistence.Configurations;

public sealed class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockBalance> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.ReservedQuantity).HasPrecision(18, 4);
        builder.HasIndex(x => new { x.CompanyId, x.ProductId }).IsUnique();
    }
}

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property(x => x.Type).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Reference).HasMaxLength(150);
        builder.HasIndex(x => new { x.CompanyId, x.ProductId, x.CreatedAtUtc });
    }
}

public sealed class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.SourceLocation).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DestinationLocation).HasMaxLength(100).IsRequired();
    }
}

public sealed class StockCountConfiguration : IEntityTypeConfiguration<StockCount>
{
    public void Configure(EntityTypeBuilder<StockCount> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExpectedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.CountedQuantity).HasPrecision(18, 4);
        builder.HasIndex(x => new { x.CompanyId, x.ProductId, x.CountedAtUtc });
    }
}
