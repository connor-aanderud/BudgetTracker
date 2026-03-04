using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Application.DTOs;

public class UpdateMerchantRequest
{
    [Required]
    [StringLength(200)]
    public string NormalizedName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string MatchPattern { get; set; } = string.Empty;

    [Required]
    public string MatchType { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }
}