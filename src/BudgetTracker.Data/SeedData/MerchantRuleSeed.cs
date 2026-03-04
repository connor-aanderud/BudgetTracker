using BudgetTracker.Domain.Entities;
using BudgetTracker.Domain.Enums;

namespace BudgetTracker.Data.SeedData;

public static class MerchantRuleSeed
{
    public static Merchant[] GetMerchantRules()
    {
        return
        [
            new Merchant { Id = 1, NormalizedName = "Scheels Campus", MatchPattern = "SCHEELS CAMPUS", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 1 },
            new Merchant { Id = 2, NormalizedName = "Scheels HomeHardware", MatchPattern = "SCHEELS HOMEHARDWARE", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 1 },
            new Merchant { Id = 3, NormalizedName = "Scheels Fargo", MatchPattern = "SCHEELS FARGO", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 1 },
            new Merchant { Id = 4, NormalizedName = "Cenex", MatchPattern = "CENEX", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 2 },
            new Merchant { Id = 5, NormalizedName = "Simonson Gas", MatchPattern = "SIMONSON GAS", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 2 },
            new Merchant { Id = 6, NormalizedName = "Holiday Stations", MatchPattern = "HOLIDAY STATION", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 2 },
            new Merchant { Id = 7, NormalizedName = "Casey's", MatchPattern = "CASEYS", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 3 },
            new Merchant { Id = 8, NormalizedName = "Target", MatchPattern = "TARGET", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 4 },
            new Merchant { Id = 9, NormalizedName = "Walmart", MatchPattern = "WAL-MART", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 4 },
            new Merchant { Id = 10, NormalizedName = "Dollar General", MatchPattern = "DOLLAR GENERAL", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 4 },
            new Merchant { Id = 11, NormalizedName = "Starbucks", MatchPattern = "STARBUCKS", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 12, NormalizedName = "Chick-fil-A", MatchPattern = "CHICK-FIL-A", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 13, NormalizedName = "Taco Bell", MatchPattern = "TACO BELL", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 14, NormalizedName = "Hornbacher's", MatchPattern = "HORNBACHER", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 8 },
            new Merchant { Id = 15, NormalizedName = "Spotify", MatchPattern = "SPOTIFY", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 9 },
            new Merchant { Id = 16, NormalizedName = "TouchTunes", MatchPattern = "TOUCHTUNES", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 7 },
            new Merchant { Id = 17, NormalizedName = "Fargo Billiards", MatchPattern = "FARGO BILLIARDS", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 7 },
            new Merchant { Id = 18, NormalizedName = "Happy Harry's", MatchPattern = "HAPPY HARRY", MatchType = Domain.Enums.MatchType.Contains, CategoryId = 10 },
        ];
    }
}