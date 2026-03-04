using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Application.DTOs;

public class ReassignCategoryRequest
{
    [Required]
    public int CategoryId { get; set; }

    public bool? CreateRule { get; set; }

    public string? MerchantName { get; set; }

    public string? MatchPattern { get; set; }

    public string? MatchType { get; set; }
}