namespace BudgetTracker.Application.DTOs;

public class IncomeDto
{
    public int Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public bool IsRecurring { get; set; }
    public string? Frequency { get; set; }
    public int? TransactionId { get; set; }
    public string? Notes { get; set; }
}