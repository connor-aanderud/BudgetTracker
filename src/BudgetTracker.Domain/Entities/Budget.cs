namespace BudgetTracker.Domain.Entities;

public class Budget
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Month { get; set; } = string.Empty;
    public decimal LimitAmount { get; set; }

    public Category Category { get; set; } = null!;
}
