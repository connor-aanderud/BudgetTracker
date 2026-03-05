using System.Text;
using Asp.Versioning;
using AutoMapper;
using BudgetTracker.Application.Common;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using MatchType = BudgetTracker.Domain.Enums.MatchType;

namespace BudgetTracker.API.Controllers.v1;

/// <summary>
/// Manages transactions.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IMerchantRepository _merchantRepo;
    private readonly IMapper _mapper;

    public TransactionsController(
        ITransactionRepository transactionRepo,
        ICategoryRepository categoryRepo,
        IMerchantRepository merchantRepo,
        IMapper mapper)
    {
        _transactionRepo = transactionRepo;
        _categoryRepo = categoryRepo;
        _merchantRepo = merchantRepo;
        _mapper = mapper;
    }

    /// <summary>
    /// Get a filtered, paginated list of transactions.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TransactionDto>>>> GetAll(
        [FromQuery] TransactionFilterParams filters,
        CancellationToken cancellationToken)
    {
        var result = await _transactionRepo.GetFilteredAsync(filters, cancellationToken);
        var dtos = _mapper.Map<List<TransactionDto>>(result.Items);

        var pagedDto = new PagedResult<TransactionDto>
        {
            Items = dtos,
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        return Ok(ApiResponse<PagedResult<TransactionDto>>.Ok(pagedDto));
    }

    /// <summary>
    /// Bulk-update the category for multiple transactions.
    /// </summary>
    [HttpPut("bulk-categorize")]
    public async Task<ActionResult<ApiResponse<object>>> BulkCategorize(
        [FromBody] BulkCategorizeRequest request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return BadRequest(ApiResponse<object>.Fail($"Category with ID {request.CategoryId} not found."));

        await _transactionRepo.BulkUpdateCategoryAsync(request.Ids, request.CategoryId, cancellationToken);
        await _transactionRepo.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<object>.Ok(null!, $"{request.Ids.Count} transaction(s) categorized."));
    }

    /// <summary>
    /// Bulk-delete multiple transactions by ID.
    /// </summary>
    [HttpDelete("bulk-delete")]
    public async Task<ActionResult<ApiResponse<object>>> BulkDelete(
        [FromBody] BulkDeleteRequest request,
        CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepo.FindAsync(t => request.Ids.Contains(t.Id), cancellationToken);
        if (transactions.Count == 0)
            return NotFound(ApiResponse<object>.Fail("No transactions found for the provided IDs."));

        foreach (var transaction in transactions)
            _transactionRepo.Remove(transaction);

        await _transactionRepo.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<object>.Ok(null!, $"{transactions.Count} transaction(s) deleted."));
    }

    /// <summary>
    /// Reassign a transaction's category, optionally creating a merchant rule.
    /// </summary>
    [HttpPut("{id:int}/category")]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> ReassignCategory(
        int id,
        [FromBody] ReassignCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepo.GetByIdAsync(id, cancellationToken);
        if (transaction is null)
            return NotFound(ApiResponse<TransactionDto>.Fail($"Transaction with ID {id} not found."));

        var category = await _categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return BadRequest(ApiResponse<TransactionDto>.Fail($"Category with ID {request.CategoryId} not found."));

        transaction.CategoryId = request.CategoryId;
        transaction.Category = category;
        transaction.ManuallyRecategorized = true;

        if (request.CreateRule == true)
        {
            if (string.IsNullOrWhiteSpace(request.MatchPattern))
                return BadRequest(ApiResponse<TransactionDto>.Fail("MatchPattern is required when creating a merchant rule."));

            var matchTypeStr = request.MatchType ?? "Contains";
            if (!Enum.TryParse<MatchType>(matchTypeStr, ignoreCase: true, out var matchType))
                return BadRequest(ApiResponse<TransactionDto>.Fail(
                    $"Invalid MatchType '{matchTypeStr}'. Valid values: {string.Join(", ", Enum.GetNames<MatchType>())}"));

            var merchant = new Merchant
            {
                NormalizedName = request.MerchantName ?? request.MatchPattern,
                MatchPattern = request.MatchPattern,
                MatchType = matchType,
                CategoryId = request.CategoryId
            };

            await _merchantRepo.AddAsync(merchant, cancellationToken);
            transaction.Merchant = merchant;
        }

        _transactionRepo.Update(transaction);
        await _transactionRepo.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<TransactionDto>(transaction);
        return Ok(ApiResponse<TransactionDto>.Ok(dto, "Transaction category reassigned."));
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] TransactionFilterParams filters,
        CancellationToken cancellationToken)
    {
        // Force no pagination — fetch all matching transactions
        filters.Page = 1;
        filters.PageSize = int.MaxValue;

        var result = await _transactionRepo.GetFilteredAsync(filters, cancellationToken);
        var transactions = result.Items;

        var categories = await _categoryRepo.GetAllAsync(cancellationToken);
        var categoryLookup = categories.ToDictionary(c => c.Id, c => c.Name);

        var merchants = await _merchantRepo.GetAllAsync(cancellationToken);
        var merchantLookup = merchants.ToDictionary(m => m.Id, m => m.NormalizedName);

        var sb = new StringBuilder();
        sb.AppendLine("Date,Description,Category,Merchant,Amount,Type");

        foreach (var t in transactions)
        {
            var categoryName = t.CategoryId.HasValue && categoryLookup.TryGetValue(t.CategoryId.Value, out var catName)
                ? catName
                : "Uncategorized";

            var merchantName = t.MerchantId.HasValue && merchantLookup.TryGetValue(t.MerchantId.Value, out var merName)
                ? merName
                : "";

            var description = EscapeCsvField(t.RawDescription);
            var escapedCategory = EscapeCsvField(categoryName);
            var escapedMerchant = EscapeCsvField(merchantName);
            var type = t.IsCredit ? "Credit" : "Debit";

            sb.AppendLine($"{t.TransactionDate:yyyy-MM-dd},{description},{escapedCategory},{escapedMerchant},{t.Amount},{type}");
        }

        var now = DateTime.Now;
        var filename = $"transactions_{now:yyyy-MM}.csv";

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", filename);
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}