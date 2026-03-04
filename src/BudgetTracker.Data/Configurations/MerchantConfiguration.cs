using BudgetTracker.Data.SeedData;
using BudgetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetTracker.Data.Configurations;

public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.NormalizedName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.MatchPattern)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(m => m.Category)
            .WithMany(c => c.Merchants)
            .HasForeignKey(m => m.CategoryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.NormalizedName);

        builder.HasData(MerchantRuleSeed.GetMerchantRules());
    }
}
