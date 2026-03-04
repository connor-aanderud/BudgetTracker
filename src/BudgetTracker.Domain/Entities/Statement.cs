namespace BudgetTracker.Domain.Entities;

public class Statement
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}