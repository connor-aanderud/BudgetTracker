using BudgetTracker.Application.DTOs;

namespace BudgetTracker.Application.Interfaces;

public interface IAnalyticsService
{
    Task<MonthlySummaryDto> GetMonthlySummaryAsync(string month, CancellationToken cancellationToken = default);
    Task<List<CategoryBreakdownDto>> GetCategoryBreakdownAsync(string month, CancellationToken cancellationToken = default);
    Task<List<BudgetStatusDto>> GetBudgetStatusAsync(string month, CancellationToken cancellationToken = default);
    Task<List<TopMerchantDto>> GetTopMerchantsAsync(int months, int limit, CancellationToken cancellationToken = default);
    Task<List<MonthlyComparisonDto>> GetMonthlyComparisonAsync(string month, CancellationToken cancellationToken = default);
}
