namespace BudgetTracker.Application.DTOs;

public class ParseResultDto
{
    public string FileName { get; set; } = string.Empty;
    public string? Source { get; set; }
    public List<ParsedTransactionDto> ParsedTransactions { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public int TotalCount { get; set; }
    public int DuplicateCount { get; set; }
}

public class ParsedTransactionDto
{
    public DateOnly TransactionDate { get; set; }
    public DateOnly? PostDate { get; set; }
    public string RawDescription { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsCredit { get; set; }
    public string DuplicateHash { get; set; } = string.Empty;
    public bool IsDuplicate { get; set; }
}
