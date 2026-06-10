using Asp.Versioning;
using AutoMapper;
using BudgetTracker.Application.Common;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using BudgetTracker.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.API.Controllers.v1;

/// <summary>
/// Manages statement uploads, parsing, and import confirmation.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class StatementsController : ControllerBase
{
    private readonly StatementService _statementService;
    private readonly IStatementRepository _statementRepo;
    private readonly IMapper _mapper;

    public StatementsController(
        StatementService statementService,
        IStatementRepository statementRepo,
        IMapper mapper)
    {
        _statementService = statementService;
        _statementRepo = statementRepo;
        _mapper = mapper;
    }

    /// <summary>
    /// Upload a CSV or PDF statement and get a preview of parsed transactions.
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<ParseResultDto>>> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<ParseResultDto>.Fail("No file provided."));

        var isSupported = file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
            || file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        if (!isSupported)
            return BadRequest(ApiResponse<ParseResultDto>.Fail("Only CSV and PDF files are supported."));

        using var stream = file.OpenReadStream();
        var result = await _statementService.ParseUploadAsync(stream, file.FileName);

        if (result.ParsedTransactions.Count == 0)
            return Ok(ApiResponse<ParseResultDto>.Fail("No transactions found in file.", result.Warnings));

        return Ok(ApiResponse<ParseResultDto>.Ok(result));
    }

    /// <summary>
    /// Confirm import of previously parsed transactions.
    /// </summary>
    [HttpPost("confirm")]
    public async Task<ActionResult<ApiResponse<StatementDto>>> Confirm([FromBody] ParseResultDto parseResult, CancellationToken cancellationToken)
    {
        if (parseResult.ParsedTransactions.Count == 0)
            return BadRequest(ApiResponse<StatementDto>.Fail("No transactions to import."));

        var statement = await _statementService.ConfirmImportAsync(parseResult, cancellationToken);
        return Ok(ApiResponse<StatementDto>.Ok(statement, $"Imported {statement.TransactionCount} transactions."));
    }

    /// <summary>
    /// Get all uploaded statements.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StatementDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var statements = await _statementRepo.GetAllAsync(cancellationToken);
        var dtos = _mapper.Map<List<StatementDto>>(statements);
        return Ok(ApiResponse<List<StatementDto>>.Ok(dtos));
    }
}