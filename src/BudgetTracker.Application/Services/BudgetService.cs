using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;

namespace BudgetTracker.Application.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly ICategoryRepository _categoryRepo;

    public BudgetService(
        IBudgetRepository budgetRepo,
        ITransactionRepository transactionRepo,
        ICategoryRepository categoryRepo)
    {
        _budgetRepo = budgetRepo;
        _transactionRepo = transactionRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<List<BudgetStatusDto>> GetBudgetStatusAsync(string month, CancellationToken cancellationToken = default)
    {
        var budgets = await _budgetRepo.GetByMonthAsync(month, cancellationToken);

        // Parse month string "YYYY-MM" to get date range
        var parsed = DateOnly.ParseExact(month, "yyyy-MM");
        var startDate = new DateOnly(parsed.Year, parsed.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Get all transactions in this month (debits only, not credits)
        var transactions = await _transactionRepo.FindAsync(
            t => t.TransactionDate >= startDate
                 && t.TransactionDate <= endDate
                 && !t.IsCredit,
            cancellationToken);

        // Group transactions by CategoryId and sum amounts
        var spendingByCategory = transactions
            .Where(t => t.CategoryId.HasValue)
            .GroupBy(t => t.CategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(t => Math.Abs(t.Amount)));

        var result = budgets.Select(b =>
        {
            var actual = spendingByCategory.GetValueOrDefault(b.CategoryId, 0m);
            var remaining = b.LimitAmount - actual;

            return new BudgetStatusDto
            {
                CategoryId = b.CategoryId,
                CategoryName = b.Category?.Name ?? string.Empty,
                CategoryColor = b.Category?.Color ?? string.Empty,
                LimitAmount = b.LimitAmount,
                ActualAmount = actual,
                RemainingAmount = remaining,
                OverBudget = remaining < 0,
                PercentUsed = b.LimitAmount > 0
                    ? (double)(actual / b.LimitAmount) * 100
                    : 0
            };
        }).ToList();

        return result;
    }

    public async Task CopyBudgetsAsync(string fromMonth, string toMonth, CancellationToken cancellationToken = default)
    {
        await _budgetRepo.CopyBudgetsAsync(fromMonth, toMonth, cancellationToken);
    }
}
