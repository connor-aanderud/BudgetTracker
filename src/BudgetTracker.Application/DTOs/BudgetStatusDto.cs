namespace BudgetTracker.Application.DTOs;

public class BudgetStatusDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public decimal LimitAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public bool OverBudget { get; set; }
    public double PercentUsed { get; set; }
}
