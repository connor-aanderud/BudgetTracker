using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Application.DTOs;

public class BulkDeleteRequest
{
    [Required]
    public List<int> Ids { get; set; } = new();
}