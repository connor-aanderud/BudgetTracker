namespace BudgetTracker.Application.DTOs;

public class TransactionFilterParams
{
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public List<int>? CategoryIds { get; set; }
    public int? MerchantId { get; set; }
    public int? StatementId { get; set; }
    public decimal? AmountMin { get; set; }
    public decimal? AmountMax { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}