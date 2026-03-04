namespace BudgetTracker.Application.DTOs;

public class BudgetDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryColor { get; set; }
    public string Month { get; set; } = string.Empty;
    public decimal LimitAmount { get; set; }
}
