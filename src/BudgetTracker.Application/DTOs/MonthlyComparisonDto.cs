namespace BudgetTracker.Application.DTOs;

public class MonthlyComparisonDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public decimal CurrentMonth { get; set; }
    public decimal PreviousMonth { get; set; }
    public decimal ThreeMonthAverage { get; set; }
}
