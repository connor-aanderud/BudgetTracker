using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Application.DTOs;

public class BulkCategorizeRequest
{
    [Required]
    public List<int> Ids { get; set; } = new();

    [Required]
    public int CategoryId { get; set; }
}