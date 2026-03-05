using Asp.Versioning;
using BudgetTracker.Application.Common;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("monthly-summary")]
    public async Task<ActionResult<ApiResponse<MonthlySummaryDto>>> GetMonthlySummary(
        [FromQuery] string month,
        CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetMonthlySummaryAsync(month, cancellationToken);
        return Ok(ApiResponse<MonthlySummaryDto>.Ok(result));
    }

    [HttpGet("category-breakdown")]
    public async Task<ActionResult<ApiResponse<List<CategoryBreakdownDto>>>> GetCategoryBreakdown(
        [FromQuery] string month,
        CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetCategoryBreakdownAsync(month, cancellationToken);
        return Ok(ApiResponse<List<CategoryBreakdownDto>>.Ok(result));
    }

    [HttpGet("budget-status")]
    public async Task<ActionResult<ApiResponse<List<BudgetStatusDto>>>> GetBudgetStatus(
        [FromQuery] string month,
        CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetBudgetStatusAsync(month, cancellationToken);
        return Ok(ApiResponse<List<BudgetStatusDto>>.Ok(result));
    }

    [HttpGet("trends")]
    public async Task<ActionResult<ApiResponse<List<SpendingTrendDto>>>> GetSpendingTrends(
        [FromQuery] int months = 6,
        [FromQuery] int? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetSpendingTrendsAsync(months, categoryId, cancellationToken);
        return Ok(ApiResponse<List<SpendingTrendDto>>.Ok(result));
    }

    [HttpGet("top-merchants")]
    public async Task<ActionResult<ApiResponse<List<TopMerchantDto>>>> GetTopMerchants(
        [FromQuery] int months = 3,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetTopMerchantsAsync(months, limit, cancellationToken);
        return Ok(ApiResponse<List<TopMerchantDto>>.Ok(result));
    }

    [HttpGet("monthly-comparison")]
    public async Task<ActionResult<ApiResponse<List<MonthlyComparisonDto>>>> GetMonthlyComparison(
        [FromQuery] string month,
        CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetMonthlyComparisonAsync(month, cancellationToken);
        return Ok(ApiResponse<List<MonthlyComparisonDto>>.Ok(result));
    }
}
