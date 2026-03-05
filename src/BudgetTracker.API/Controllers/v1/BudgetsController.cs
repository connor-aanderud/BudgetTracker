using Asp.Versioning;
using AutoMapper;
using BudgetTracker.Application.Common;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetRepository _budgetRepo;
    private readonly IBudgetService _budgetService;
    private readonly IMapper _mapper;

    public BudgetsController(
        IBudgetRepository budgetRepo,
        IBudgetService budgetService,
        IMapper mapper)
    {
        _budgetRepo = budgetRepo;
        _budgetService = budgetService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BudgetDto>>>> GetByMonth(
        [FromQuery] string month,
        CancellationToken cancellationToken)
    {
        var budgets = await _budgetRepo.GetByMonthAsync(month, cancellationToken);
        var dtos = _mapper.Map<List<BudgetDto>>(budgets);
        return Ok(ApiResponse<List<BudgetDto>>.Ok(dtos));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<List<BudgetDto>>>> SetBudgets(
        [FromBody] SetBudgetRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await _budgetRepo.GetByMonthAsync(request.Month, cancellationToken);
        var existingDict = existing.ToDictionary(b => b.CategoryId);

        var requestedCategoryIds = request.Budgets.Select(b => b.CategoryId).ToHashSet();

        // Remove budgets for categories no longer in the request
        foreach (var budget in existing)
        {
            if (!requestedCategoryIds.Contains(budget.CategoryId))
                _budgetRepo.Remove(budget);
        }

        // Upsert each line item
        foreach (var lineItem in request.Budgets)
        {
            if (existingDict.TryGetValue(lineItem.CategoryId, out var existingBudget))
            {
                existingBudget.LimitAmount = lineItem.LimitAmount;
                _budgetRepo.Update(existingBudget);
            }
            else
            {
                var newBudget = new Budget
                {
                    CategoryId = lineItem.CategoryId,
                    Month = request.Month,
                    LimitAmount = lineItem.LimitAmount
                };
                await _budgetRepo.AddAsync(newBudget, cancellationToken);
            }
        }

        await _budgetRepo.SaveChangesAsync(cancellationToken);

        // Reload with category navigation for proper DTO mapping
        var updated = await _budgetRepo.GetByMonthAsync(request.Month, cancellationToken);
        var dtos = _mapper.Map<List<BudgetDto>>(updated);
        return Ok(ApiResponse<List<BudgetDto>>.Ok(dtos, "Budgets updated."));
    }

    [HttpPost("copy")]
    public async Task<ActionResult<ApiResponse<bool>>> CopyBudgets(
        [FromBody] CopyBudgetsRequest request,
        CancellationToken cancellationToken)
    {
        await _budgetService.CopyBudgetsAsync(request.FromMonth, request.ToMonth, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, $"Budgets copied from {request.FromMonth} to {request.ToMonth}."));
    }

    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<List<BudgetStatusDto>>>> GetBudgetStatus(
        [FromQuery] string month,
        CancellationToken cancellationToken)
    {
        var status = await _budgetService.GetBudgetStatusAsync(month, cancellationToken);
        return Ok(ApiResponse<List<BudgetStatusDto>>.Ok(status));
    }
}
