using System.Text.RegularExpressions;
using BudgetTracker.Data.SeedData;
using MatchType = BudgetTracker.Domain.Enums.MatchType;

namespace BudgetTracker.Tests.Services;

public class MerchantRuleSeedTests
{
    private static readonly Domain.Entities.Merchant[] Rules = MerchantRuleSeed.GetMerchantRules();

    [Fact]
    public void Merchants_HaveUniqueIds()
    {
        Rules.Select(m => m.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Merchants_AllReferenceAnExistingCategory()
    {
        var categoryIds = CategorySeed.GetCategories().Select(c => c.Id).ToHashSet();

        Rules.Should().OnlyContain(m => categoryIds.Contains(m.CategoryId));
    }

    [Fact]
    public void Merchants_HaveNonEmptyNameAndPattern()
    {
        Rules.Should().OnlyContain(m =>
            !string.IsNullOrWhiteSpace(m.NormalizedName) &&
            !string.IsNullOrWhiteSpace(m.MatchPattern));
    }

    [Fact]
    public void Merchants_RegexPatternsCompile()
    {
        var regexRules = Rules.Where(m => m.MatchType == MatchType.Regex);

        foreach (var rule in regexRules)
        {
            var act = () => Regex.IsMatch("PROBE", rule.MatchPattern, RegexOptions.IgnoreCase);
            act.Should().NotThrow($"merchant '{rule.NormalizedName}' has pattern '{rule.MatchPattern}'");
        }
    }
}
