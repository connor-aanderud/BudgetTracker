using BudgetTracker.Domain.Entities;

namespace BudgetTracker.Application.Interfaces;

public interface IStatementRepository : IRepository<Statement>
{
    Task<Statement?> GetWithTransactionsAsync(int id, CancellationToken cancellationToken = default);
}
