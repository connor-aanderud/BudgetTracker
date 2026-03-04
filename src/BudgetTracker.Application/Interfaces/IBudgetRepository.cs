using BudgetTracker.Domain.Entities;

namespace BudgetTracker.Application.Interfaces;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<IReadOnlyList<Budget>> GetByMonthAsync(string month, CancellationToken cancellationToken = default);
    Task CopyBudgetsAsync(string fromMonth, string toMonth, CancellationToken cancellationToken = default);
}