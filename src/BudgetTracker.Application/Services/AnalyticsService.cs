using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;

namespace BudgetTracker.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly IIncomeRepository _incomeRepo;
    private readonly IBudgetService _budgetService;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IMerchantRepository _merchantRepo;

    public AnalyticsService(
        ITransactionRepository transactionRepo,
        IIncomeRepository incomeRepo,
        IBudgetService budgetService,
        ICategoryRepository categoryRepo,
        IMerchantRepository merchantRepo)
    {
        _transactionRepo = transactionRepo;
        _incomeRepo = incomeRepo;
        _budgetService = budgetService;
        _categoryRepo = categoryRepo;
        _merchantRepo = merchantRepo;
    }

    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(string month, CancellationToken cancellationToken = default)
    {
        var parsed = DateOnly.ParseExact(month, "yyyy-MM");
        var startDate = new DateOnly(parsed.Year, parsed.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var transactions = await _transactionRepo.FindAsync(
            t => t.TransactionDate >= startDate
                 && t.TransactionDate <= endDate
                 && !t.IsCredit,
            cancellationToken);

        var totalExpenses = transactions.Sum(t => Math.Abs(t.Amount));

        var incomes = await _incomeRepo.FindAsync(
            i => i.Date >= startDate && i.Date <= endDate,
            cancellationToken);

        var totalIncome = incomes.Sum(i => i.Amount);

        // Previous month
        var prevStart = startDate.AddMonths(-1);
        var prevEnd = startDate.AddDays(-1);

        var prevTransactions = await _transactionRepo.FindAsync(
            t => t.TransactionDate >= prevStart
                 && t.TransactionDate <= prevEnd
                 && !t.IsCredit,
            cancellationToken);

        decimal? previousMonthExpenses = prevTransactions.Any()
            ? prevTransactions.Sum(t => Math.Abs(t.Amount))
            : null;

        double? percentChange = previousMonthExpenses.HasValue && previousMonthExpenses.Value > 0
            ? (double)((totalExpenses - previousMonthExpenses.Value) / previousMonthExpenses.Value) * 100
            : null;

        return new MonthlySummaryDto
        {
            Month = month,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetSavings = totalIncome - totalExpenses,
            PreviousMonthExpenses = previousMonthExpenses,
            PercentChangeExpenses = percentChange
        };
    }

    public async Task<List<CategoryBreakdownDto>> GetCategoryBreakdownAsync(string month, CancellationToken cancellationToken = default)
    {
        var parsed = DateOnly.ParseExact(month, "yyyy-MM");
        var startDate = new DateOnly(parsed.Year, parsed.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var transactions = await _transactionRepo.FindAsync(
            t => t.TransactionDate >= startDate
                 && t.TransactionDate <= endDate
                 && !t.IsCredit,
            cancellationToken);

        var categories = await _categoryRepo.GetAllAsync(cancellationToken);
        var categoryLookup = categories.ToDictionary(c => c.Id, c => c);

        var totalAmount = transactions.Sum(t => Math.Abs(t.Amount));

        var grouped = transactions
            .GroupBy(t => t.CategoryId ?? 0)
            .Select(g =>
            {
                var catId = g.Key;
                categoryLookup.TryGetValue(catId, out var category);
                var amount = g.Sum(t => Math.Abs(t.Amount));

                return new CategoryBreakdownDto
                {
                    CategoryId = catId,
                    CategoryName = category?.Name ?? "Uncategorized",
                    CategoryColor = category?.Color ?? "#9E9E9E",
                    TotalAmount = amount,
                    TransactionCount = g.Count(),
                    PercentOfTotal = totalAmount > 0
                        ? (double)(amount / totalAmount) * 100
                        : 0
                };
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        return grouped;
    }

    public async Task<List<BudgetStatusDto>> GetBudgetStatusAsync(string month, CancellationToken cancellationToken = default)
    {
        return await _budgetService.GetBudgetStatusAsync(month, cancellationToken);
    }

    public async Task<List<SpendingTrendDto>> GetSpendingTrendsAsync(int months, int? categoryId, CancellationToken cancellationToken = default)
    {
        var now = DateOnly.FromDateTime(DateTime.Today);
        var endDate = new DateOnly(now.Year, now.Month, 1).AddMonths(1).AddDays(-1);
        var startDate = new DateOnly(now.Year, now.Month, 1).AddMonths(-(months - 1));

        var transactions = categoryId.HasValue
            ? await _transactionRepo.FindAsync(
                t => t.TransactionDate >= startDate
                     && t.TransactionDate <= endDate
                     && !t.IsCredit
                     && t.CategoryId == categoryId.Value,
                cancellationToken)
            : await _transactionRepo.FindAsync(
                t => t.TransactionDate >= startDate
                     && t.TransactionDate <= endDate
                     && !t.IsCredit,
                cancellationToken);

        string? categoryName = null;
        if (categoryId.HasValue)
        {
            var category = await _categoryRepo.GetByIdAsync(categoryId.Value, cancellationToken);
            categoryName = category?.Name;
        }

        var grouped = transactions
            .GroupBy(t => $"{t.TransactionDate.Year:D4}-{t.TransactionDate.Month:D2}")
            .Select(g => new SpendingTrendDto
            {
                Month = g.Key,
                TotalAmount = g.Sum(t => Math.Abs(t.Amount)),
                CategoryId = categoryId,
                CategoryName = categoryName
            })
            .OrderBy(s => s.Month)
            .ToList();

        return grouped;
    }

    public async Task<List<TopMerchantDto>> GetTopMerchantsAsync(int months, int limit, CancellationToken cancellationToken = default)
    {
        var now = DateOnly.FromDateTime(DateTime.Today);
        var endDate = new DateOnly(now.Year, now.Month, 1).AddMonths(1).AddDays(-1);
        var startDate = new DateOnly(now.Year, now.Month, 1).AddMonths(-(months - 1));

        var transactions = await _transactionRepo.FindAsync(
            t => t.TransactionDate >= startDate
                 && t.TransactionDate <= endDate
                 && !t.IsCredit
                 && t.MerchantId != null,
            cancellationToken);

        var merchants = await _merchantRepo.GetAllAsync(cancellationToken);
        var merchantLookup = merchants.ToDictionary(m => m.Id, m => m);

        var grouped = transactions
            .GroupBy(t => t.MerchantId!.Value)
            .Select(g =>
            {
                merchantLookup.TryGetValue(g.Key, out var merchant);
                return new TopMerchantDto
                {
                    MerchantId = g.Key,
                    MerchantName = merchant?.NormalizedName ?? "Unknown",
                    TotalAmount = g.Sum(t => Math.Abs(t.Amount)),
                    TransactionCount = g.Count()
                };
            })
            .OrderByDescending(m => m.TotalAmount)
            .Take(limit)
            .ToList();

        return grouped;
    }

    public async Task<List<MonthlyComparisonDto>> GetMonthlyComparisonAsync(string month, CancellationToken cancellationToken = default)
    {
        var parsed = DateOnly.ParseExact(month, "yyyy-MM");
        var currentStart = new DateOnly(parsed.Year, parsed.Month, 1);
        var currentEnd = currentStart.AddMonths(1).AddDays(-1);

        var prevStart = currentStart.AddMonths(-1);
        var prevEnd = currentStart.AddDays(-1);

        var threeMonthStart = currentStart.AddMonths(-3);
        var threeMonthEnd = currentStart.AddDays(-1);

        var allStart = threeMonthStart;
        var allEnd = currentEnd;

        var transactions = await _transactionRepo.FindAsync(
            t => t.TransactionDate >= allStart
                 && t.TransactionDate <= allEnd
                 && !t.IsCredit,
            cancellationToken);

        var categories = await _categoryRepo.GetAllAsync(cancellationToken);
        var categoryLookup = categories.ToDictionary(c => c.Id, c => c);

        var currentTransactions = transactions
            .Where(t => t.TransactionDate >= currentStart && t.TransactionDate <= currentEnd);
        var prevTransactions = transactions
            .Where(t => t.TransactionDate >= prevStart && t.TransactionDate <= prevEnd);
        var threeMonthTransactions = transactions
            .Where(t => t.TransactionDate >= threeMonthStart && t.TransactionDate <= threeMonthEnd);

        var currentByCategory = currentTransactions
            .GroupBy(t => t.CategoryId ?? 0)
            .ToDictionary(g => g.Key, g => g.Sum(t => Math.Abs(t.Amount)));

        var prevByCategory = prevTransactions
            .GroupBy(t => t.CategoryId ?? 0)
            .ToDictionary(g => g.Key, g => g.Sum(t => Math.Abs(t.Amount)));

        var threeMonthByCategory = threeMonthTransactions
            .GroupBy(t => t.CategoryId ?? 0)
            .ToDictionary(g => g.Key, g => g.Sum(t => Math.Abs(t.Amount)));

        var allCategoryIds = currentByCategory.Keys
            .Union(prevByCategory.Keys)
            .Union(threeMonthByCategory.Keys)
            .ToHashSet();

        var result = allCategoryIds.Select(catId =>
        {
            categoryLookup.TryGetValue(catId, out var category);
            return new MonthlyComparisonDto
            {
                CategoryId = catId,
                CategoryName = category?.Name ?? "Uncategorized",
                CategoryColor = category?.Color ?? "#9E9E9E",
                CurrentMonth = currentByCategory.GetValueOrDefault(catId, 0m),
                PreviousMonth = prevByCategory.GetValueOrDefault(catId, 0m),
                ThreeMonthAverage = threeMonthByCategory.GetValueOrDefault(catId, 0m) / 3m
            };
        })
        .OrderByDescending(c => c.CurrentMonth)
        .ToList();

        return result;
    }
}
