using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;

namespace BudgetTracker.Application.Services;

public class StatementService
{
    private readonly IStatementParser _parser;
    private readonly IStatementRepository _statementRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly ICategorizationService _categorizationService;

    public StatementService(
        IStatementParser parser,
        IStatementRepository statementRepo,
        ITransactionRepository transactionRepo,
        ICategorizationService categorizationService)
    {
        _parser = parser;
        _statementRepo = statementRepo;
        _transactionRepo = transactionRepo;
        _categorizationService = categorizationService;
    }

    public async Task<ParseResultDto> ParseUploadAsync(Stream fileStream, string fileName)
    {
        var result = await _parser.ParseAsync(fileStream, fileName);

        // Check for duplicates against existing data
        var duplicateCount = 0;
        foreach (var tx in result.ParsedTransactions)
        {
            var existing = await _transactionRepo.GetByDuplicateHashAsync(tx.DuplicateHash);
            if (existing != null)
            {
                tx.IsDuplicate = true;
                duplicateCount++;
            }
        }
        result.DuplicateCount = duplicateCount;

        return result;
    }

    public async Task<StatementDto> ConfirmImportAsync(ParseResultDto parseResult, CancellationToken cancellationToken = default)
    {
        var statement = new Statement
        {
            FileName = parseResult.FileName,
            UploadDate = DateTime.UtcNow,
            Source = parseResult.Source ?? "Unknown"
        };

        await _statementRepo.AddAsync(statement, cancellationToken);
        await _statementRepo.SaveChangesAsync(cancellationToken);

        var transactions = parseResult.ParsedTransactions
            .Where(t => !t.IsDuplicate)
            .Select(t => new Transaction
            {
                TransactionDate = t.TransactionDate,
                PostDate = t.PostDate,
                RawDescription = t.RawDescription,
                Amount = t.Amount,
                IsCredit = t.IsCredit,
                DuplicateHash = t.DuplicateHash,
                ManuallyRecategorized = false,
                StatementId = statement.Id
            })
            .ToList();

        if (transactions.Count > 0)
        {
            await _transactionRepo.AddRangeAsync(transactions, cancellationToken);

            // Auto-categorize transactions based on merchant rules
            await _categorizationService.CategorizeAsync(transactions, cancellationToken);

            await _transactionRepo.SaveChangesAsync(cancellationToken);
        }

        // Set period from transaction dates
        if (transactions.Count > 0)
        {
            statement.PeriodStart = transactions.Min(t => t.TransactionDate);
            statement.PeriodEnd = transactions.Max(t => t.TransactionDate);
            _statementRepo.Update(statement);
            await _statementRepo.SaveChangesAsync(cancellationToken);
        }

        return new StatementDto
        {
            Id = statement.Id,
            FileName = statement.FileName,
            UploadDate = statement.UploadDate,
            Source = statement.Source,
            PeriodStart = statement.PeriodStart,
            PeriodEnd = statement.PeriodEnd,
            TransactionCount = transactions.Count
        };
    }
}