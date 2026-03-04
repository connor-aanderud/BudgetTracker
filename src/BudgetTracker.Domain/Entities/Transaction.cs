namespace BudgetTracker.Domain.Entities;

public class Transaction
{
    public int Id { get; set; }
    public DateOnly TransactionDate { get; set; }
    public DateOnly? PostDate { get; set; }
    public string RawDescription { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsCredit { get; set; }
    public string DuplicateHash { get; set; } = string.Empty;
    public bool ManuallyRecategorized { get; set; }

    public int StatementId { get; set; }
    public int? CategoryId { get; set; }
    public int? MerchantId { get; set; }

    public Statement Statement { get; set; } = null!;
    public Category? Category { get; set; }
    public Merchant? Merchant { get; set; }
}