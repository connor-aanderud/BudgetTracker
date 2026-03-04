using BudgetTracker.Domain.Entities;

namespace BudgetTracker.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetWithMerchantsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllWithCountsAsync(CancellationToken cancellationToken = default);
}