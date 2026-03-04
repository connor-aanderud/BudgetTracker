using BudgetTracker.Domain.Entities;

namespace BudgetTracker.Data.SeedData;

public static class CategorySeed
{
    public static Category[] GetCategories()
    {
        return
        [
            new Category { Id = 1, Name = "Scheels", Color = "#0066CC", IsDefault = true },
            new Category { Id = 2, Name = "Gas & Fuel", Color = "#FF6600", IsDefault = true },
            new Category { Id = 3, Name = "Gas & Convenience", Color = "#FF9933", IsDefault = true },
            new Category { Id = 4, Name = "Retail", Color = "#9933CC", IsDefault = true },
            new Category { Id = 5, Name = "Dining", Color = "#FF3366", IsDefault = true },
            new Category { Id = 6, Name = "Bars & Nightlife", Color = "#CC0066", IsDefault = true },
            new Category { Id = 7, Name = "Entertainment", Color = "#FF66CC", IsDefault = true },
            new Category { Id = 8, Name = "Groceries", Color = "#33CC33", IsDefault = true },
            new Category { Id = 9, Name = "Subscriptions", Color = "#3399FF", IsDefault = true },
            new Category { Id = 10, Name = "Liquor Store", Color = "#993300", IsDefault = true },
            new Category { Id = 11, Name = "Health & Medical", Color = "#00CC99", IsDefault = true },
            new Category { Id = 12, Name = "Auto", Color = "#666666", IsDefault = true },
            new Category { Id = 13, Name = "Personal Care", Color = "#FF99CC", IsDefault = true },
            new Category { Id = 14, Name = "Donations", Color = "#FFCC00", IsDefault = true },
            new Category { Id = 15, Name = "Other", Color = "#999999", IsDefault = true },
        ];
    }
}
