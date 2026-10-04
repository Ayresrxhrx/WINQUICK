using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WinQuick.Core.Entities;

namespace WinQuick.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityName).HasMaxLength(150).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.EntityName, x.EntityId });
        builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
    }
}
