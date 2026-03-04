using Asp.Versioning;
using AutoMapper;
using BudgetTracker.Application.Common;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.API.Controllers.v1;

/// <summary>
/// Manages budget categories.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IMapper _mapper;

    public CategoriesController(
        ICategoryRepository categoryRepo,
        ITransactionRepository transactionRepo,
        IMapper mapper)
    {
        _categoryRepo = categoryRepo;
        _transactionRepo = transactionRepo;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all categories with transaction and merchant counts.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _categoryRepo.GetAllWithCountsAsync(cancellationToken);
        var dtos = _mapper.Map<List<CategoryDto>>(categories);
        return Ok(ApiResponse<List<CategoryDto>>.Ok(dtos));
    }

    /// <summary>
    /// Create a new category.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Name = request.Name,
            Color = request.Color,
            Icon = request.Icon
        };

        await _categoryRepo.AddAsync(category, cancellationToken);
        await _categoryRepo.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<CategoryDto>(category);
        return CreatedAtAction(nameof(GetAll), null, ApiResponse<CategoryDto>.Ok(dto, "Category created."));
    }

    /// <summary>
    /// Update an existing category.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(
        int id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepo.GetByIdAsync(id, cancellationToken);
        if (category is null)
            return NotFound(ApiResponse<CategoryDto>.Fail($"Category with ID {id} not found."));

        category.Name = request.Name;
        category.Color = request.Color;
        category.Icon = request.Icon;

        _categoryRepo.Update(category);
        await _categoryRepo.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<CategoryDto>(category);
        return Ok(ApiResponse<CategoryDto>.Ok(dto, "Category updated."));
    }

    /// <summary>
    /// Delete a category. Fails if the category has associated transactions.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id, CancellationToken cancellationToken)
    {
        var category = await _categoryRepo.GetByIdAsync(id, cancellationToken);
        if (category is null)
            return NotFound(ApiResponse<bool>.Fail($"Category with ID {id} not found."));

        var transactions = await _transactionRepo.FindAsync(t => t.CategoryId == id, cancellationToken);
        if (transactions.Count > 0)
            return BadRequest(ApiResponse<bool>.Fail(
                $"Cannot delete category '{category.Name}' because it has {transactions.Count} associated transaction(s). Reassign them first."));

        _categoryRepo.Remove(category);
        await _categoryRepo.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<bool>.Ok(true, "Category deleted."));
    }
}