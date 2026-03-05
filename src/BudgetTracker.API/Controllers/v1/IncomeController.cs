using Asp.Versioning;
using AutoMapper;
using BudgetTracker.Application.Common;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Frequency = BudgetTracker.Domain.Enums.Frequency;

namespace BudgetTracker.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class IncomeController : ControllerBase
{
    private readonly IIncomeRepository _incomeRepo;
    private readonly IMapper _mapper;

    public IncomeController(
        IIncomeRepository incomeRepo,
        IMapper mapper)
    {
        _incomeRepo = incomeRepo;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<IncomeDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var incomes = await _incomeRepo.GetAllAsync(cancellationToken);
        var dtos = _mapper.Map<List<IncomeDto>>(incomes);
        return Ok(ApiResponse<List<IncomeDto>>.Ok(dtos));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<IncomeDto>>> Create(
        [FromBody] CreateIncomeRequest request,
        CancellationToken cancellationToken)
    {
        Frequency? frequency = null;
        if (request.Frequency is not null)
        {
            if (!Enum.TryParse<Frequency>(request.Frequency, ignoreCase: true, out var parsed))
                return BadRequest(ApiResponse<IncomeDto>.Fail(
                    $"Invalid Frequency '{request.Frequency}'. Valid values: {string.Join(", ", Enum.GetNames<Frequency>())}"));
            frequency = parsed;
        }

        var income = new Income
        {
            Source = request.Source,
            Amount = request.Amount,
            Date = request.Date,
            IsRecurring = request.IsRecurring,
            Frequency = frequency,
            Notes = request.Notes
        };

        await _incomeRepo.AddAsync(income, cancellationToken);
        await _incomeRepo.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<IncomeDto>(income);
        return CreatedAtAction(nameof(GetAll), null, ApiResponse<IncomeDto>.Ok(dto, "Income created."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<IncomeDto>>> Update(
        int id,
        [FromBody] UpdateIncomeRequest request,
        CancellationToken cancellationToken)
    {
        Frequency? frequency = null;
        if (request.Frequency is not null)
        {
            if (!Enum.TryParse<Frequency>(request.Frequency, ignoreCase: true, out var parsed))
                return BadRequest(ApiResponse<IncomeDto>.Fail(
                    $"Invalid Frequency '{request.Frequency}'. Valid values: {string.Join(", ", Enum.GetNames<Frequency>())}"));
            frequency = parsed;
        }

        var income = await _incomeRepo.GetByIdAsync(id, cancellationToken);
        if (income is null)
            return NotFound(ApiResponse<IncomeDto>.Fail($"Income with ID {id} not found."));

        income.Source = request.Source;
        income.Amount = request.Amount;
        income.Date = request.Date;
        income.IsRecurring = request.IsRecurring;
        income.Frequency = frequency;
        income.Notes = request.Notes;

        _incomeRepo.Update(income);
        await _incomeRepo.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<IncomeDto>(income);
        return Ok(ApiResponse<IncomeDto>.Ok(dto, "Income updated."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id, CancellationToken cancellationToken)
    {
        var income = await _incomeRepo.GetByIdAsync(id, cancellationToken);
        if (income is null)
            return NotFound(ApiResponse<bool>.Fail($"Income with ID {id} not found."));

        _incomeRepo.Remove(income);
        await _incomeRepo.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<bool>.Ok(true, "Income deleted."));
    }
}
