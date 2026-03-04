namespace BudgetTracker.Application.DTOs;

public class MerchantDto
{
    public int Id { get; set; }
    public string NormalizedName { get; set; } = string.Empty;
    public string MatchPattern { get; set; } = string.Empty;
    public string MatchType { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
}