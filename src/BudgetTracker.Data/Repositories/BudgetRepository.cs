using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Data.Repositories;

public class BudgetRepository : Repository<Budget>, IBudgetRepository
{
    public BudgetRepository(BudgetDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Budget>> GetByMonthAsync(string month, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Category)
            .Where(b => b.Month == month)
            .ToListAsync(cancellationToken);
    }

    public async Task CopyBudgetsAsync(string fromMonth, string toMonth, CancellationToken cancellationToken = default)
    {
        var sourceBudgets = await _dbSet
            .Where(b => b.Month == fromMonth)
            .ToListAsync(cancellationToken);

        var newBudgets = sourceBudgets.Select(b => new Budget
        {
            CategoryId = b.CategoryId,
            Month = toMonth,
            LimitAmount = b.LimitAmount,
        });

        await _dbSet.AddRangeAsync(newBudgets, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}