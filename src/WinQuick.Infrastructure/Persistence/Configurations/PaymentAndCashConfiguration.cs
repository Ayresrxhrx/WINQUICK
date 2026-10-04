using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WinQuick.Core.Entities;

namespace WinQuick.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AmountApplied).HasPrecision(18, 4);
        builder.Property(x => x.AmountTendered).HasPrecision(18, 4);
        builder.Property(x => x.ChangeAmount).HasPrecision(18, 4);
        builder.Property(x => x.Reference).HasMaxLength(150);
        builder.HasIndex(x => new { x.CompanyId, x.PaidAtUtc });
        builder.HasIndex(x => x.SaleId);
        builder.HasIndex(x => x.InvoiceId);
    }
}

public sealed class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
    }
}

public sealed class CashSessionConfiguration : IEntityTypeConfiguration<CashSession>
{
    public void Configure(EntityTypeBuilder<CashSession> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OpeningAmount).HasPrecision(18, 4);
        builder.Property(x => x.ExpectedAmount).HasPrecision(18, 4);
        builder.Property(x => x.CountedAmount).HasPrecision(18, 4);
        builder.Property(x => x.DifferenceAmount).HasPrecision(18, 4);
        builder.HasIndex(x => new { x.TerminalId, x.IsOpen });
    }
}

public sealed class CashMovementConfiguration : IEntityTypeConfiguration<CashMovement>
{
    public void Configure(EntityTypeBuilder<CashMovement> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasPrecision(18, 4);
        builder.Property(x => x.Type).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => new { x.CashSessionId, x.CreatedAtUtc });
    }
}
