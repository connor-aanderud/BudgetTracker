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
}