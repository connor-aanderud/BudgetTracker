using BudgetTracker.Domain.Enums;

namespace BudgetTracker.Domain.Entities;

public class Income
{
    public int Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public bool IsRecurring { get; set; }
    public Frequency? Frequency { get; set; }
    public int? TransactionId { get; set; }
    public string? Notes { get; set; }

    public Transaction? Transaction { get; set; }
}