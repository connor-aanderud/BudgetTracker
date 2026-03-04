using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Data.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(BudgetDbContext context) : base(context) { }

    public async Task<Category?> GetWithMerchantsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Merchants)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllWithCountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Transactions)
            .Include(c => c.Merchants)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}