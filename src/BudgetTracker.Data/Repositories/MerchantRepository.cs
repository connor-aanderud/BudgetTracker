using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Data.Repositories;

public class MerchantRepository : Repository<Merchant>, IMerchantRepository
{
    public MerchantRepository(BudgetDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Merchant>> GetAllWithCategoryAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.Category)
            .OrderBy(m => m.NormalizedName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Merchant?> FindMatchingMerchantAsync(string description, CancellationToken cancellationToken = default)
    {
        var merchants = await _dbSet
            .Include(m => m.Category)
            .ToListAsync(cancellationToken);

        var upperDescription = description.ToUpperInvariant();

        // Priority: Exact > Contains > Regex
        var exact = merchants.FirstOrDefault(m =>
            m.MatchType == Domain.Enums.MatchType.Exact &&
            upperDescription.Equals(m.MatchPattern, StringComparison.OrdinalIgnoreCase));
        if (exact != null) return exact;

        var contains = merchants.FirstOrDefault(m =>
            m.MatchType == Domain.Enums.MatchType.Contains &&
            upperDescription.Contains(m.MatchPattern, StringComparison.OrdinalIgnoreCase));
        if (contains != null) return contains;

        var regex = merchants.FirstOrDefault(m =>
            m.MatchType == Domain.Enums.MatchType.Regex &&
            System.Text.RegularExpressions.Regex.IsMatch(upperDescription, m.MatchPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase));

        return regex;
    }
}