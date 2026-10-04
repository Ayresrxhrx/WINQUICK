using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WinQuick.Core.Entities;

namespace WinQuick.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Nuit).HasMaxLength(30);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.CreditLimit).HasPrecision(18, 4);
        builder.Property(x => x.CreditBalance).HasPrecision(18, 4);
        builder.HasIndex(x => new { x.CompanyId, x.Nuit });
        builder.HasIndex(x => new { x.CompanyId, x.Phone });
        builder.HasIndex(x => new { x.CompanyId, x.Name });
    }
}
