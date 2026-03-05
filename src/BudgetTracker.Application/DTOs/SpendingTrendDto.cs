namespace BudgetTracker.Application.DTOs;

public class SpendingTrendDto
{
    public string Month { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
}
