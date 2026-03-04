namespace BudgetTracker.Application.DTOs;

public class StatementDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public int TransactionCount { get; set; }
}
