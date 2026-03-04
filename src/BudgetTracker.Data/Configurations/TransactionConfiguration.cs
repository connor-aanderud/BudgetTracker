using BudgetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetTracker.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.RawDescription)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2);

        builder.Property(t => t.DuplicateHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(t => t.DuplicateHash)
            .IsUnique();

        builder.HasOne(t => t.Statement)
            .WithMany(s => s.Transactions)
            .HasForeignKey(t => t.StatementId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Merchant)
            .WithMany(m => m.Transactions)
            .HasForeignKey(t => t.MerchantId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => t.TransactionDate);
        builder.HasIndex(t => t.CategoryId);
        builder.HasIndex(t => t.StatementId);
    }
}
