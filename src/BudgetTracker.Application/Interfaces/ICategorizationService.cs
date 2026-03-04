using BudgetTracker.Domain.Entities;

namespace BudgetTracker.Application.Interfaces;

public interface ICategorizationService
{
    Task CategorizeAsync(List<Transaction> transactions, CancellationToken cancellationToken = default);
}
