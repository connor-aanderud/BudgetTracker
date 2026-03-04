using BudgetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetTracker.Data.Configurations;

public class IncomeConfiguration : IEntityTypeConfiguration<Income>
{
    public void Configure(EntityTypeBuilder<Income> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Source)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Amount)
            .HasPrecision(18, 2);

        builder.Property(i => i.Notes)
            .HasMaxLength(500);

        builder.HasOne(i => i.Transaction)
            .WithOne()
            .HasForeignKey<Income>(i => i.TransactionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
