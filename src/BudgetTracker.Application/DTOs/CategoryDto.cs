namespace BudgetTracker.Application.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsDefault { get; set; }
    public int TransactionCount { get; set; }
    public int MerchantCount { get; set; }
}
