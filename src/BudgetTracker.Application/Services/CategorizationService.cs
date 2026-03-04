using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;

namespace BudgetTracker.Application.Services;

public class CategorizationService : ICategorizationService
{
    private readonly IMerchantRepository _merchantRepo;

    public CategorizationService(IMerchantRepository merchantRepo)
    {
        _merchantRepo = merchantRepo;
    }

    public async Task CategorizeAsync(List<Transaction> transactions, CancellationToken cancellationToken = default)
    {
        foreach (var transaction in transactions)
        {
            if (transaction.ManuallyRecategorized)
                continue;

            var merchant = await _merchantRepo.FindMatchingMerchantAsync(
                transaction.RawDescription, cancellationToken);

            if (merchant != null)
            {
                transaction.MerchantId = merchant.Id;
                transaction.CategoryId = merchant.CategoryId;
            }
        }
    }
}