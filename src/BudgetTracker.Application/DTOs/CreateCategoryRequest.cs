using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Application.DTOs;

public class CreateCategoryRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(7)]
    public string Color { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Icon { get; set; }
}