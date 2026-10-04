using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WinQuick.Core.Entities;

namespace WinQuick.Infrastructure.Persistence.Configurations;

public sealed class CashMovementConfiguration : IEntityTypeConfiguration<CashMovement>
{
    public void Configure(EntityTypeBuilder<CashMovement> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasPrecision(18, 4);
        builder.Property(x => x.Type).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => new { x.CashSessionId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.CompanyId, x.TerminalId, x.CreatedAtUtc });
    }
}
