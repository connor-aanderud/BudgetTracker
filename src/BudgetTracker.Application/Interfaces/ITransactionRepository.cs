using BudgetTracker.Application.Common;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Domain.Entities;

namespace BudgetTracker.Application.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<PagedResult<Transaction>> GetFilteredAsync(TransactionFilterParams filters, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByDuplicateHashAsync(string hash, CancellationToken cancellationToken = default);
    Task BulkUpdateCategoryAsync(List<int> ids, int categoryId, CancellationToken cancellationToken = default);
}