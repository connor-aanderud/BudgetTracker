using BudgetTracker.Application.Common;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Data.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(BudgetDbContext context) : base(context) { }

    public async Task<PagedResult<Transaction>> GetFilteredAsync(TransactionFilterParams filters, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(t => t.Category)
            .Include(t => t.Merchant)
            .Include(t => t.Statement)
            .AsQueryable();

        if (filters.DateFrom.HasValue)
            query = query.Where(t => t.TransactionDate >= filters.DateFrom.Value);
        if (filters.DateTo.HasValue)
            query = query.Where(t => t.TransactionDate <= filters.DateTo.Value);
        if (filters.CategoryIds is { Count: > 0 })
            query = query.Where(t => t.CategoryId.HasValue && filters.CategoryIds.Contains(t.CategoryId.Value));
        if (filters.MerchantId.HasValue)
            query = query.Where(t => t.MerchantId == filters.MerchantId.Value);
        if (filters.StatementId.HasValue)
            query = query.Where(t => t.StatementId == filters.StatementId.Value);
        if (filters.AmountMin.HasValue)
            query = query.Where(t => t.Amount >= filters.AmountMin.Value);
        if (filters.AmountMax.HasValue)
            query = query.Where(t => t.Amount <= filters.AmountMax.Value);
        if (!string.IsNullOrWhiteSpace(filters.Search))
            query = query.Where(t => t.RawDescription.Contains(filters.Search));

        query = filters.SortBy?.ToLowerInvariant() switch
        {
            "amount" => filters.SortDir == "asc" ? query.OrderBy(t => t.Amount) : query.OrderByDescending(t => t.Amount),
            "description" => filters.SortDir == "asc" ? query.OrderBy(t => t.RawDescription) : query.OrderByDescending(t => t.RawDescription),
            "category" => filters.SortDir == "asc" ? query.OrderBy(t => t.Category!.Name) : query.OrderByDescending(t => t.Category!.Name),
            _ => filters.SortDir == "asc" ? query.OrderBy(t => t.TransactionDate) : query.OrderByDescending(t => t.TransactionDate),
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Transaction>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filters.Page,
            PageSize = filters.PageSize,
        };
    }

    public async Task<Transaction?> GetByDuplicateHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.DuplicateHash == hash, cancellationToken);
    }

    public async Task BulkUpdateCategoryAsync(List<int> ids, int categoryId, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(t => ids.Contains(t.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.CategoryId, categoryId)
                .SetProperty(t => t.ManuallyRecategorized, true),
                cancellationToken);
    }
}