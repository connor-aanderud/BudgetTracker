namespace BudgetTracker.Application.DTOs;

public class TransactionDto
{
    public int Id { get; set; }
    public DateOnly TransactionDate { get; set; }
    public DateOnly? PostDate { get; set; }
    public string RawDescription { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsCredit { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? MerchantId { get; set; }
    public string? MerchantName { get; set; }
    public int StatementId { get; set; }
    public string? StatementFileName { get; set; }
    public bool ManuallyRecategorized { get; set; }
}
