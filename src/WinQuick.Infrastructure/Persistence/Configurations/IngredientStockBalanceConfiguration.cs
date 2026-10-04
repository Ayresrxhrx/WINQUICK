using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WinQuick.Core.Entities;

namespace WinQuick.Infrastructure.Persistence.Configurations;

public sealed class IngredientStockBalanceConfiguration : IEntityTypeConfiguration<IngredientStockBalance>
{
    public void Configure(EntityTypeBuilder<IngredientStockBalance> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.HasIndex(x => new { x.CompanyId, x.IngredientId }).IsUnique();
        b.HasOne<Ingredient>().WithMany().HasForeignKey(x => x.IngredientId).OnDelete(DeleteBehavior.Restrict);
    }
}
