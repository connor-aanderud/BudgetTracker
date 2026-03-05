using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Application.DTOs;

public class UpdateIncomeRequest
{
    [Required]
    [StringLength(200)]
    public string Source { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    public bool IsRecurring { get; set; }

    public string? Frequency { get; set; }

    public string? Notes { get; set; }
}
