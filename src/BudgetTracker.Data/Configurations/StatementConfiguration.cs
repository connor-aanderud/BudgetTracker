using BudgetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetTracker.Data.Configurations;

public class StatementConfiguration : IEntityTypeConfiguration<Statement>
{
    public void Configure(EntityTypeBuilder<Statement> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.Source)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(s => s.Source);
    }
}
