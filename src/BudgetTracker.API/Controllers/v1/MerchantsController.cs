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
/// Manages merchant matching rules.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MerchantsController : ControllerBase
{
    private readonly IMerchantRepository _merchantRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IMapper _mapper;

    public MerchantsController(
        IMerchantRepository merchantRepo,
        ICategoryRepository categoryRepo,
        IMapper mapper)
    {
        _merchantRepo = merchantRepo;
        _categoryRepo = categoryRepo;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all merchant rules with category name.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<MerchantDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var merchants = await _merchantRepo.GetAllWithCategoryAsync(cancellationToken);
        var dtos = _mapper.Map<List<MerchantDto>>(merchants);
        return Ok(ApiResponse<List<MerchantDto>>.Ok(dtos));
    }

    /// <summary>
    /// Create a new merchant rule.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<MerchantDto>>> Create(
        [FromBody] CreateMerchantRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<MatchType>(request.MatchType, ignoreCase: true, out var matchType))
            return BadRequest(ApiResponse<MerchantDto>.Fail(
                $"Invalid MatchType '{request.MatchType}'. Valid values: {string.Join(", ", Enum.GetNames<MatchType>())}"));

        var category = await _categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return BadRequest(ApiResponse<MerchantDto>.Fail($"Category with ID {request.CategoryId} not found."));

        var merchant = new Merchant
        {
            NormalizedName = request.NormalizedName,
            MatchPattern = request.MatchPattern,
            MatchType = matchType,
            CategoryId = request.CategoryId
        };

        await _merchantRepo.AddAsync(merchant, cancellationToken);
        await _merchantRepo.SaveChangesAsync(cancellationToken);

        // Reload with category for proper DTO mapping
        merchant.Category = category;
        var dto = _mapper.Map<MerchantDto>(merchant);
        return CreatedAtAction(nameof(GetAll), null, ApiResponse<MerchantDto>.Ok(dto, "Merchant rule created."));
    }

    /// <summary>
    /// Update an existing merchant rule.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<MerchantDto>>> Update(
        int id,
        [FromBody] UpdateMerchantRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<MatchType>(request.MatchType, ignoreCase: true, out var matchType))
            return BadRequest(ApiResponse<MerchantDto>.Fail(
                $"Invalid MatchType '{request.MatchType}'. Valid values: {string.Join(", ", Enum.GetNames<MatchType>())}"));

        var merchant = await _merchantRepo.GetByIdAsync(id, cancellationToken);
        if (merchant is null)
            return NotFound(ApiResponse<MerchantDto>.Fail($"Merchant rule with ID {id} not found."));

        var category = await _categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null)
            return BadRequest(ApiResponse<MerchantDto>.Fail($"Category with ID {request.CategoryId} not found."));

        merchant.NormalizedName = request.NormalizedName;
        merchant.MatchPattern = request.MatchPattern;
        merchant.MatchType = matchType;
        merchant.CategoryId = request.CategoryId;

        _merchantRepo.Update(merchant);
        await _merchantRepo.SaveChangesAsync(cancellationToken);

        merchant.Category = category;
        var dto = _mapper.Map<MerchantDto>(merchant);
        return Ok(ApiResponse<MerchantDto>.Ok(dto, "Merchant rule updated."));
    }

    /// <summary>
    /// Delete a merchant rule.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id, CancellationToken cancellationToken)
    {
        var merchant = await _merchantRepo.GetByIdAsync(id, cancellationToken);
        if (merchant is null)
            return NotFound(ApiResponse<bool>.Fail($"Merchant rule with ID {id} not found."));

        _merchantRepo.Remove(merchant);
        await _merchantRepo.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<bool>.Ok(true, "Merchant rule deleted."));
    }
}