using BudgetTracker.Application.DTOs;

namespace BudgetTracker.Application.Interfaces;

public interface IBudgetService
{
    Task<List<BudgetStatusDto>> GetBudgetStatusAsync(string month, CancellationToken cancellationToken = default);
    Task CopyBudgetsAsync(string fromMonth, string toMonth, CancellationToken cancellationToken = default);
}
