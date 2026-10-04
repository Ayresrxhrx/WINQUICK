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
