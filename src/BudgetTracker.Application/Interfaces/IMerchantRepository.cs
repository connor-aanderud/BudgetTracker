using BudgetTracker.Domain.Entities;

namespace BudgetTracker.Application.Interfaces;

public interface IMerchantRepository : IRepository<Merchant>
{
    Task<IReadOnlyList<Merchant>> GetAllWithCategoryAsync(CancellationToken cancellationToken = default);
    Task<Merchant?> FindMatchingMerchantAsync(string description, CancellationToken cancellationToken = default);
}