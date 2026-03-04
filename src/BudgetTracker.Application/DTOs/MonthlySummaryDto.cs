namespace BudgetTracker.Application.DTOs;

public class MonthlySummaryDto
{
    public string Month { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetSavings { get; set; }
    public decimal? PreviousMonthExpenses { get; set; }
    public double? PercentChangeExpenses { get; set; }
}
