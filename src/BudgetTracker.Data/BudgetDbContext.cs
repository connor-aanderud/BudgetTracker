using BudgetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Data;

public class BudgetDbContext : DbContext
{
    public DbSet<Statement> Statements => Set<Statement>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Income> Incomes => Set<Income>();
    public DbSet<Budget> Budgets => Set<Budget>();

    public BudgetDbContext(DbContextOptions<BudgetDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BudgetDbContext).Assembly);
    }
}
