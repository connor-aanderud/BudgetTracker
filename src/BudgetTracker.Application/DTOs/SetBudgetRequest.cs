using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Application.DTOs;

public class SetBudgetRequest
{
    [Required]
    public string Month { get; set; } = string.Empty;

    [Required]
    public List<BudgetLineItem> Budgets { get; set; } = [];
}

public class BudgetLineItem
{
    [Required]
    public int CategoryId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal LimitAmount { get; set; }
}
