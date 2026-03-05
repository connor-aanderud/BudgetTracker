using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Application.DTOs;

public class CopyBudgetsRequest
{
    [Required]
    public string FromMonth { get; set; } = string.Empty;

    [Required]
    public string ToMonth { get; set; } = string.Empty;
}
