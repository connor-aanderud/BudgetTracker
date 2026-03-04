using BudgetTracker.Domain.Enums;

namespace BudgetTracker.Domain.Entities;

public class Merchant
{
    public int Id { get; set; }
    public string NormalizedName { get; set; } = string.Empty;
    public string MatchPattern { get; set; } = string.Empty;
    public Enums.MatchType MatchType { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}