using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Data.Repositories;

public class StatementRepository : Repository<Statement>, IStatementRepository
{
    public StatementRepository(BudgetDbContext context) : base(context) { }

    public async Task<Statement?> GetWithTransactionsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Transactions)
                .ThenInclude(t => t.Category)
            .Include(s => s.Transactions)
                .ThenInclude(t => t.Merchant)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}