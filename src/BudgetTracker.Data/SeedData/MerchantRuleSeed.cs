using BudgetTracker.Domain.Entities;
using MatchType = BudgetTracker.Domain.Enums.MatchType;

namespace BudgetTracker.Data.SeedData;

public static class MerchantRuleSeed
{
    // Category ids (see CategorySeed):
    // 1 Scheels · 2 Gas & Fuel · 3 Gas & Convenience · 4 Retail · 5 Dining ·
    // 6 Bars & Nightlife · 7 Entertainment · 8 Groceries · 9 Subscriptions ·
    // 10 Liquor Store · 11 Health & Medical · 12 Auto · 13 Personal Care ·
    // 14 Donations · 15 Other · 16 Pets & Vet · 17 Phone & Utilities ·
    // 18 Travel & Transportation · 19 Taxes & Fees · 20 Home Improvement
    //
    // Patterns are matched case-insensitively against the raw transaction description.
    // Priority is Exact > Contains > Regex, then first-match-wins in list order — so
    // more specific rules are listed before broader fallbacks (e.g. named sports bars
    // before the generic "SPORTS BAR").
    public static Merchant[] GetMerchantRules()
    {
        return
        [
            // Scheels — "Scheels Campus" is its own merchant; Fargo / HomeHardware /
            // All Sports are grouped into one. (Campus is a Contains rule so it wins
            // over the regex by priority.)
            new Merchant { Id = 1, NormalizedName = "Scheels Campus", MatchPattern = "SCHEELS CAMPUS", MatchType = MatchType.Contains, CategoryId = 1 },
            new Merchant { Id = 2, NormalizedName = "Scheels", MatchPattern = "SCHEELS FARGO|SCHEELS HOMEHARDWARE|SCHEELS ALL SPORTS", MatchType = MatchType.Regex, CategoryId = 1 },

            // Gas & Fuel
            new Merchant { Id = 3, NormalizedName = "Cenex", MatchPattern = "CENEX", MatchType = MatchType.Contains, CategoryId = 2 },
            new Merchant { Id = 4, NormalizedName = "Simonson Gas", MatchPattern = "SIMONSON GAS", MatchType = MatchType.Contains, CategoryId = 2 },
            new Merchant { Id = 5, NormalizedName = "Holiday", MatchPattern = "HOLIDAY STATION|CK HOLIDAY", MatchType = MatchType.Regex, CategoryId = 2 },
            new Merchant { Id = 6, NormalizedName = "Petro Serve USA", MatchPattern = "PETRO SERVE|PETRO GAS", MatchType = MatchType.Regex, CategoryId = 2 },

            // Gas & Convenience
            new Merchant { Id = 7, NormalizedName = "Casey's", MatchPattern = "CASEYS", MatchType = MatchType.Contains, CategoryId = 3 },
            new Merchant { Id = 8, NormalizedName = "Circle K", MatchPattern = "CIRCLE K", MatchType = MatchType.Contains, CategoryId = 3 },
            new Merchant { Id = 9, NormalizedName = "Kwik Star", MatchPattern = "KWIK STAR", MatchType = MatchType.Contains, CategoryId = 3 },
            new Merchant { Id = 10, NormalizedName = "Love's", MatchPattern = "LOVE'S", MatchType = MatchType.Contains, CategoryId = 3 },

            // Retail
            new Merchant { Id = 11, NormalizedName = "Target", MatchPattern = "TARGET", MatchType = MatchType.Contains, CategoryId = 4 },
            new Merchant { Id = 12, NormalizedName = "Walmart", MatchPattern = "WAL-?MART|WM SUPERCENTER", MatchType = MatchType.Regex, CategoryId = 4 },
            new Merchant { Id = 13, NormalizedName = "Dollar General", MatchPattern = "DOLLAR GENERAL", MatchType = MatchType.Contains, CategoryId = 4 },
            new Merchant { Id = 14, NormalizedName = "Amazon", MatchPattern = "AMAZON", MatchType = MatchType.Contains, CategoryId = 4 },
            new Merchant { Id = 15, NormalizedName = "Buckle", MatchPattern = "BUCKLE", MatchType = MatchType.Contains, CategoryId = 4 },
            new Merchant { Id = 16, NormalizedName = "TJ Maxx", MatchPattern = "TJMAXX", MatchType = MatchType.Contains, CategoryId = 4 },
            new Merchant { Id = 17, NormalizedName = "Hobby Lobby", MatchPattern = "HOBBY LOBBY", MatchType = MatchType.Contains, CategoryId = 4 },
            new Merchant { Id = 18, NormalizedName = "Fleet Farm", MatchPattern = "FLEET FARM", MatchType = MatchType.Contains, CategoryId = 4 },
            new Merchant { Id = 19, NormalizedName = "ZAGG", MatchPattern = "ZAGG", MatchType = MatchType.Contains, CategoryId = 4 },
            new Merchant { Id = 20, NormalizedName = "Babylist", MatchPattern = "BABYLIST", MatchType = MatchType.Contains, CategoryId = 4 },
            new Merchant { Id = 21, NormalizedName = "InstaFreshener", MatchPattern = "INSTAFRESHENER", MatchType = MatchType.Contains, CategoryId = 4 },
            new Merchant { Id = 22, NormalizedName = "Books Are Fun", MatchPattern = "BOOKSAREFUN", MatchType = MatchType.Contains, CategoryId = 4 },

            // Dining
            new Merchant { Id = 23, NormalizedName = "Starbucks", MatchPattern = "STARBUCKS", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 24, NormalizedName = "Chick-fil-A", MatchPattern = "CHICK-FIL-A", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 25, NormalizedName = "Taco Bell", MatchPattern = "TACO BELL", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 26, NormalizedName = "7 Brew Coffee", MatchPattern = "BREW COFFEE", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 27, NormalizedName = "Kobe's Japanese", MatchPattern = "KOBE'S", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 28, NormalizedName = "Osaka Sushi", MatchPattern = "OSAKA SUSHI", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 29, NormalizedName = "Red Pepper", MatchPattern = "RED PEPPER", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 30, NormalizedName = "El Agave", MatchPattern = "EL AGAVE", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 31, NormalizedName = "Qdoba", MatchPattern = "QDOBA", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 32, NormalizedName = "Subway", MatchPattern = "SUBWAY", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 33, NormalizedName = "McDonald's", MatchPattern = "MCDONALD", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 34, NormalizedName = "Slim Chickens", MatchPattern = "SLIM CHICKENS", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 35, NormalizedName = "Burger King", MatchPattern = "BURGER KING", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 36, NormalizedName = "Papa Murphy's", MatchPattern = "PAPA MURPHY", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 37, NormalizedName = "Crisp & Green", MatchPattern = "CRISP & GREEN", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 38, NormalizedName = "Cold Stone Creamery", MatchPattern = "COLD STONE", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 39, NormalizedName = "Sandy's Donuts", MatchPattern = "SANDY'S DONUTS", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 40, NormalizedName = "Queen's Quesadilla", MatchPattern = "QUEEN'S QUESADILLA", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 41, NormalizedName = "Rocky's Burgers", MatchPattern = "ROCKY'S", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 42, NormalizedName = "Chili's", MatchPattern = "CHILIS", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 43, NormalizedName = "Sickies Garage", MatchPattern = "SICKIES", MatchType = MatchType.Contains, CategoryId = 5 },
            new Merchant { Id = 44, NormalizedName = "Uber Eats", MatchPattern = "UBER *EATS", MatchType = MatchType.Contains, CategoryId = 5 },

            // Bars & Nightlife — named venues before the generic "SPORTS BAR" fallback
            new Merchant { Id = 45, NormalizedName = "Hooligans", MatchPattern = "HOOLIGANS", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 46, NormalizedName = "Twin Peaks", MatchPattern = "TWIN PEAKS", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 47, NormalizedName = "Shy Bar", MatchPattern = "SHY BAR", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 48, NormalizedName = "Lucky's 13", MatchPattern = "LUCKY'S 13", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 49, NormalizedName = "PubWest", MatchPattern = "PUBWEST", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 50, NormalizedName = "Cowboy Jacks", MatchPattern = "COWBOY JACKS", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 51, NormalizedName = "Bulldog Tap", MatchPattern = "BULLDOG", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 52, NormalizedName = "Top Hat Lounge", MatchPattern = "TOP HAT", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 53, NormalizedName = "Legends Sports Bar", MatchPattern = "LEGENDS SPORTS", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 54, NormalizedName = "Rookies Sports Bar", MatchPattern = "ROOKIES", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 55, NormalizedName = "Brewtus Brickhouse", MatchPattern = "BREWTUS", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 56, NormalizedName = "The Bison Turf", MatchPattern = "BISON TURF", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 57, NormalizedName = "Wild Bills", MatchPattern = "WILD BILLS", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 58, NormalizedName = "Northern Taphouse", MatchPattern = "NORTHERN TAPHOUSE", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 59, NormalizedName = "Tap That", MatchPattern = "TAP THAT", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 60, NormalizedName = "Vets Club", MatchPattern = "VETS CLUB", MatchType = MatchType.Contains, CategoryId = 6 },
            new Merchant { Id = 61, NormalizedName = "Sports Bar", MatchPattern = "SPORTS BAR", MatchType = MatchType.Contains, CategoryId = 6 },

            // Entertainment
            new Merchant { Id = 62, NormalizedName = "TouchTunes", MatchPattern = "TOUCHTUNES", MatchType = MatchType.Contains, CategoryId = 7 },
            new Merchant { Id = 63, NormalizedName = "Fargo Billiards", MatchPattern = "FARGO BILLIARDS", MatchType = MatchType.Contains, CategoryId = 7 },
            new Merchant { Id = 64, NormalizedName = "Slick City", MatchPattern = "SLICKCITY", MatchType = MatchType.Contains, CategoryId = 7 },
            new Merchant { Id = 65, NormalizedName = "Mayville Golf Club", MatchPattern = "MAYVILLE GOLF", MatchType = MatchType.Contains, CategoryId = 7 },
            new Merchant { Id = 66, NormalizedName = "Golden Lake", MatchPattern = "GOLDEN LAKE", MatchType = MatchType.Contains, CategoryId = 7 },
            new Merchant { Id = 67, NormalizedName = "Sleeper", MatchPattern = "SLEEPER", MatchType = MatchType.Contains, CategoryId = 7 },

            // Groceries
            new Merchant { Id = 68, NormalizedName = "Hornbacher's", MatchPattern = "HORNBACHER", MatchType = MatchType.Contains, CategoryId = 8 },
            new Merchant { Id = 69, NormalizedName = "Cash Wise Foods", MatchPattern = "CASH WISE", MatchType = MatchType.Contains, CategoryId = 8 },
            new Merchant { Id = 70, NormalizedName = "Family Fare", MatchPattern = "FAMILY FARE", MatchType = MatchType.Contains, CategoryId = 8 },
            new Merchant { Id = 71, NormalizedName = "Miller's Fresh Food", MatchPattern = "MILLER'S FRESH", MatchType = MatchType.Contains, CategoryId = 8 },

            // Subscriptions
            new Merchant { Id = 72, NormalizedName = "Spotify", MatchPattern = "SPOTIFY", MatchType = MatchType.Contains, CategoryId = 9 },
            new Merchant { Id = 73, NormalizedName = "YouTube TV", MatchPattern = "YOUTUBE TV", MatchType = MatchType.Contains, CategoryId = 9 },
            new Merchant { Id = 74, NormalizedName = "Netflix", MatchPattern = "NETFLIX", MatchType = MatchType.Contains, CategoryId = 9 },
            new Merchant { Id = 75, NormalizedName = "Hulu", MatchPattern = "HULU", MatchType = MatchType.Contains, CategoryId = 9 },
            new Merchant { Id = 76, NormalizedName = "ESPN", MatchPattern = "ESPN", MatchType = MatchType.Contains, CategoryId = 9 },
            new Merchant { Id = 77, NormalizedName = "Paramount+", MatchPattern = "PARAMOUNT", MatchType = MatchType.Contains, CategoryId = 9 },
            new Merchant { Id = 78, NormalizedName = "Maestro", MatchPattern = "MAESTRO", MatchType = MatchType.Contains, CategoryId = 9 },

            // Liquor Store
            new Merchant { Id = 79, NormalizedName = "Happy Harry's", MatchPattern = "HAPPY HARRY", MatchType = MatchType.Contains, CategoryId = 10 },
            new Merchant { Id = 80, NormalizedName = "Crown Liquors", MatchPattern = "CROWN LIQUORS", MatchType = MatchType.Contains, CategoryId = 10 },
            new Merchant { Id = 81, NormalizedName = "Bottle Barn Liquors", MatchPattern = "BOTTLE BARN", MatchType = MatchType.Contains, CategoryId = 10 },

            // Health & Medical
            new Merchant { Id = 82, NormalizedName = "Rural Psychiatry", MatchPattern = "RURAL PSYCHIATRY", MatchType = MatchType.Contains, CategoryId = 11 },
            new Merchant { Id = 83, NormalizedName = "Evergreen Dental", MatchPattern = "EVERGREEN DENTAL", MatchType = MatchType.Contains, CategoryId = 11 },
            new Merchant { Id = 84, NormalizedName = "Linson Pharmacy", MatchPattern = "LINSON PHARMACY", MatchType = MatchType.Contains, CategoryId = 11 },
            new Merchant { Id = 85, NormalizedName = "Hillsboro Drug", MatchPattern = "HILLSBORO DRUG", MatchType = MatchType.Contains, CategoryId = 11 },
            new Merchant { Id = 86, NormalizedName = "Eyes on Broadway", MatchPattern = "EYES ON BROADWAY", MatchType = MatchType.Contains, CategoryId = 11 },

            // Auto
            new Merchant { Id = 87, NormalizedName = "O'Reilly Auto Parts", MatchPattern = "O'REILLY", MatchType = MatchType.Contains, CategoryId = 12 },
            new Merchant { Id = 88, NormalizedName = "Interstate Battery", MatchPattern = "INTERSTATE ALL BATTERY", MatchType = MatchType.Contains, CategoryId = 12 },
            new Merchant { Id = 89, NormalizedName = "Simonson Car Wash", MatchPattern = "SIMONSON CAR WASH", MatchType = MatchType.Contains, CategoryId = 12 },

            // Personal Care
            new Merchant { Id = 90, NormalizedName = "Nail Deluxe", MatchPattern = "NAIL DELUXE", MatchType = MatchType.Contains, CategoryId = 13 },
            new Merchant { Id = 91, NormalizedName = "Nora Salon", MatchPattern = "NORA SALON", MatchType = MatchType.Contains, CategoryId = 13 },

            // Pets & Vet
            new Merchant { Id = 92, NormalizedName = "Animal Hospital", MatchPattern = "ANIMAL HOSP", MatchType = MatchType.Contains, CategoryId = 16 },

            // Phone & Utilities
            new Merchant { Id = 93, NormalizedName = "Halstad Telephone", MatchPattern = "HALSTAD TELEPHONE", MatchType = MatchType.Contains, CategoryId = 17 },

            // Travel & Transportation
            new Merchant { Id = 94, NormalizedName = "Uber Trip", MatchPattern = "UBER *TRIP", MatchType = MatchType.Contains, CategoryId = 18 },
            new Merchant { Id = 95, NormalizedName = "Airport Parking", MatchPattern = "AIRPORT PARKING", MatchType = MatchType.Contains, CategoryId = 18 },

            // Taxes & Fees
            new Merchant { Id = 96, NormalizedName = "FreeTaxUSA", MatchPattern = "FREETAXUSA", MatchType = MatchType.Contains, CategoryId = 19 },
            new Merchant { Id = 97, NormalizedName = "ND Game & Fish", MatchPattern = "GAME & FISH", MatchType = MatchType.Contains, CategoryId = 19 },

            // Home Improvement
            new Merchant { Id = 98, NormalizedName = "Lowe's", MatchPattern = "LOWES", MatchType = MatchType.Contains, CategoryId = 20 },
            new Merchant { Id = 99, NormalizedName = "Menards", MatchPattern = "MENARDS", MatchType = MatchType.Contains, CategoryId = 20 },
        ];
    }
}
