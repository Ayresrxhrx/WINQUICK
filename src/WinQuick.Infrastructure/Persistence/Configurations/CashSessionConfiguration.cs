using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WinQuick.Core.Entities;

namespace WinQuick.Infrastructure.Persistence.Configurations;

public sealed class CashSessionConfiguration : IEntityTypeConfiguration<CashSession>
{
    public void Configure(EntityTypeBuilder<CashSession> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OpeningAmount).HasPrecision(18, 4);
        builder.Property(x => x.ExpectedAmount).HasPrecision(18, 4);
        builder.Property(x => x.CountedAmount).HasPrecision(18, 4);
        builder.Property(x => x.DifferenceAmount).HasPrecision(18, 4);
        builder.Property(x => x.ClosingJustification).HasMaxLength(1000);
        builder.HasIndex(x => new { x.CompanyId, x.TerminalId, x.IsOpen });
        builder.HasIndex(x => new { x.UserId, x.OpenedAtUtc });
    }
}
