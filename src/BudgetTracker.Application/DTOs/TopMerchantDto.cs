namespace BudgetTracker.Application.DTOs;

public class TopMerchantDto
{
    public int MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
}
