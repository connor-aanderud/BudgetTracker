using System.Linq.Expressions;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using BudgetTracker.Application.Services;
using BudgetTracker.Domain.Entities;
using NSubstitute;

namespace BudgetTracker.Tests.Services;

public class AnalyticsServiceTests
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly IIncomeRepository _incomeRepo;
    private readonly IBudgetService _budgetService;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IMerchantRepository _merchantRepo;
    private readonly AnalyticsService _service;

    public AnalyticsServiceTests()
    {
        _transactionRepo = Substitute.For<ITransactionRepository>();
        _incomeRepo = Substitute.For<IIncomeRepository>();
        _budgetService = Substitute.For<IBudgetService>();
        _categoryRepo = Substitute.For<ICategoryRepository>();
        _merchantRepo = Substitute.For<IMerchantRepository>();
        _service = new AnalyticsService(
            _transactionRepo, _incomeRepo, _budgetService, _categoryRepo, _merchantRepo);
    }

    private void SetupDefaultTransactionRepo(IReadOnlyList<Transaction>? firstCall = null, IReadOnlyList<Transaction>? secondCall = null)
    {
        var calls = 0;
        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                calls++;
                if (calls == 1) return firstCall ?? new List<Transaction>().AsReadOnly();
                return secondCall ?? new List<Transaction>().AsReadOnly();
            });
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_WithTransactionsAndIncome_ComputesTotalsCorrectly()
    {
        var currentTransactions = new List<Transaction>
        {
            new() { Id = 1, Amount = 100m, TransactionDate = new DateOnly(2025, 3, 5), IsCredit = false },
            new() { Id = 2, Amount = 50m, TransactionDate = new DateOnly(2025, 3, 15), IsCredit = false }
        }.AsReadOnly();

        var prevTransactions = new List<Transaction>
        {
            new() { Id = 3, Amount = 120m, TransactionDate = new DateOnly(2025, 2, 10), IsCredit = false }
        }.AsReadOnly();

        SetupDefaultTransactionRepo(currentTransactions, prevTransactions);

        var incomes = new List<Income>
        {
            new() { Id = 1, Amount = 3000m, Date = new DateOnly(2025, 3, 1), Source = "Salary" }
        }.AsReadOnly();

        _incomeRepo.FindAsync(Arg.Any<Expression<Func<Income, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(incomes);

        var result = await _service.GetMonthlySummaryAsync("2025-03");

        result.Month.Should().Be("2025-03");
        result.TotalExpenses.Should().Be(150m);
        result.TotalIncome.Should().Be(3000m);
        result.NetSavings.Should().Be(2850m);
        result.PreviousMonthExpenses.Should().Be(120m);
        result.PercentChangeExpenses.Should().BeApproximately(25.0, 0.01);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_NoPreviousMonthData_ReturnsNullPreviousFields()
    {
        var currentTransactions = new List<Transaction>
        {
            new() { Id = 1, Amount = 200m, TransactionDate = new DateOnly(2025, 1, 10), IsCredit = false }
        }.AsReadOnly();

        var emptyPrevious = new List<Transaction>().AsReadOnly();
        SetupDefaultTransactionRepo(currentTransactions, emptyPrevious);

        _incomeRepo.FindAsync(Arg.Any<Expression<Func<Income, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Income>().AsReadOnly());

        var result = await _service.GetMonthlySummaryAsync("2025-01");

        result.TotalExpenses.Should().Be(200m);
        result.PreviousMonthExpenses.Should().BeNull();
        result.PercentChangeExpenses.Should().BeNull();
    }

    [Fact]
    public async Task GetCategoryBreakdownAsync_GroupsByCategoryCorrectly()
    {
        var transactions = new List<Transaction>
        {
            new() { Id = 1, CategoryId = 1, Amount = 100m, TransactionDate = new DateOnly(2025, 1, 5), IsCredit = false },
            new() { Id = 2, CategoryId = 1, Amount = 50m, TransactionDate = new DateOnly(2025, 1, 10), IsCredit = false },
            new() { Id = 3, CategoryId = 2, Amount = 75m, TransactionDate = new DateOnly(2025, 1, 15), IsCredit = false }
        }.AsReadOnly();

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Groceries", Color = "#4CAF50" },
            new() { Id = 2, Name = "Dining", Color = "#FF5722" }
        }.AsReadOnly();

        _categoryRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(categories);

        var result = await _service.GetCategoryBreakdownAsync("2025-01");

        result.Should().HaveCount(2);

        var groceries = result.First(c => c.CategoryId == 1);
        groceries.CategoryName.Should().Be("Groceries");
        groceries.TotalAmount.Should().Be(150m);
        groceries.TransactionCount.Should().Be(2);

        var dining = result.First(c => c.CategoryId == 2);
        dining.CategoryName.Should().Be("Dining");
        dining.TotalAmount.Should().Be(75m);
        dining.TransactionCount.Should().Be(1);
    }

    [Fact]
    public async Task GetCategoryBreakdownAsync_CalculatesPercentages()
    {
        var transactions = new List<Transaction>
        {
            new() { Id = 1, CategoryId = 1, Amount = 75m, TransactionDate = new DateOnly(2025, 1, 5), IsCredit = false },
            new() { Id = 2, CategoryId = 2, Amount = 25m, TransactionDate = new DateOnly(2025, 1, 10), IsCredit = false }
        }.AsReadOnly();

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        _categoryRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Category>
            {
                new() { Id = 1, Name = "A", Color = "#000" },
                new() { Id = 2, Name = "B", Color = "#111" }
            }.AsReadOnly());

        var result = await _service.GetCategoryBreakdownAsync("2025-01");

        var catA = result.First(c => c.CategoryId == 1);
        catA.PercentOfTotal.Should().BeApproximately(75.0, 0.01);

        var catB = result.First(c => c.CategoryId == 2);
        catB.PercentOfTotal.Should().BeApproximately(25.0, 0.01);
    }

    [Fact]
    public async Task GetCategoryBreakdownAsync_UncategorizedTransactions_ShowAsUncategorized()
    {
        var transactions = new List<Transaction>
        {
            new() { Id = 1, CategoryId = null, Amount = 50m, TransactionDate = new DateOnly(2025, 1, 5), IsCredit = false }
        }.AsReadOnly();

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        _categoryRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Category>().AsReadOnly());

        var result = await _service.GetCategoryBreakdownAsync("2025-01");

        result.Should().ContainSingle();
        result[0].CategoryName.Should().Be("Uncategorized");
        result[0].TotalAmount.Should().Be(50m);
    }

    [Fact]
    public async Task GetBudgetStatusAsync_DelegatesToBudgetService()
    {
        var expected = new List<BudgetStatusDto>
        {
            new() { CategoryId = 1, CategoryName = "Groceries", LimitAmount = 500m, ActualAmount = 200m }
        };

        _budgetService.GetBudgetStatusAsync("2025-01", Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _service.GetBudgetStatusAsync("2025-01");

        result.Should().BeSameAs(expected);
        await _budgetService.Received(1).GetBudgetStatusAsync("2025-01", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSpendingTrendsAsync_GroupsByMonth()
    {
        var transactions = new List<Transaction>
        {
            new() { Id = 1, Amount = 100m, TransactionDate = new DateOnly(2025, 1, 5), IsCredit = false },
            new() { Id = 2, Amount = 50m, TransactionDate = new DateOnly(2025, 1, 20), IsCredit = false },
            new() { Id = 3, Amount = 200m, TransactionDate = new DateOnly(2025, 2, 10), IsCredit = false }
        }.AsReadOnly();

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        var result = await _service.GetSpendingTrendsAsync(3, null);

        result.Should().HaveCount(2);
        result[0].Month.Should().Be("2025-01");
        result[0].TotalAmount.Should().Be(150m);
        result[1].Month.Should().Be("2025-02");
        result[1].TotalAmount.Should().Be(200m);
    }

    [Fact]
    public async Task GetSpendingTrendsAsync_WithCategoryFilter_IncludesCategoryName()
    {
        var transactions = new List<Transaction>
        {
            new() { Id = 1, CategoryId = 1, Amount = 100m, TransactionDate = new DateOnly(2025, 1, 5), IsCredit = false }
        }.AsReadOnly();

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        var category = new Category { Id = 1, Name = "Groceries", Color = "#4CAF50" };
        _categoryRepo.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(category);

        var result = await _service.GetSpendingTrendsAsync(1, 1);

        result.Should().ContainSingle();
        result[0].CategoryId.Should().Be(1);
        result[0].CategoryName.Should().Be("Groceries");
    }

    [Fact]
    public async Task GetTopMerchantsAsync_ReturnsTopNOrderedByAmount()
    {
        var transactions = new List<Transaction>
        {
            new() { Id = 1, MerchantId = 1, Amount = 200m, TransactionDate = new DateOnly(2025, 1, 5), IsCredit = false },
            new() { Id = 2, MerchantId = 1, Amount = 100m, TransactionDate = new DateOnly(2025, 1, 10), IsCredit = false },
            new() { Id = 3, MerchantId = 2, Amount = 150m, TransactionDate = new DateOnly(2025, 1, 15), IsCredit = false },
            new() { Id = 4, MerchantId = 3, Amount = 50m, TransactionDate = new DateOnly(2025, 1, 20), IsCredit = false }
        }.AsReadOnly();

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        var merchants = new List<Merchant>
        {
            new() { Id = 1, NormalizedName = "Amazon", CategoryId = 1 },
            new() { Id = 2, NormalizedName = "Starbucks", CategoryId = 2 },
            new() { Id = 3, NormalizedName = "Netflix", CategoryId = 3 }
        }.AsReadOnly();

        _merchantRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(merchants);

        var result = await _service.GetTopMerchantsAsync(1, 2);

        result.Should().HaveCount(2);
        result[0].MerchantName.Should().Be("Amazon");
        result[0].TotalAmount.Should().Be(300m);
        result[0].TransactionCount.Should().Be(2);
        result[1].MerchantName.Should().Be("Starbucks");
        result[1].TotalAmount.Should().Be(150m);
    }

    [Fact]
    public async Task GetMonthlyComparisonAsync_ComputesThreeMonthAverage()
    {
        // Three months of history before current + current month
        var transactions = new List<Transaction>
        {
            // Current month: 2025-04
            new() { Id = 1, CategoryId = 1, Amount = 200m, TransactionDate = new DateOnly(2025, 4, 5), IsCredit = false },
            // Previous month: 2025-03
            new() { Id = 2, CategoryId = 1, Amount = 150m, TransactionDate = new DateOnly(2025, 3, 10), IsCredit = false },
            // Three month window: 2025-01, 2025-02, 2025-03
            new() { Id = 3, CategoryId = 1, Amount = 100m, TransactionDate = new DateOnly(2025, 1, 15), IsCredit = false },
            new() { Id = 4, CategoryId = 1, Amount = 120m, TransactionDate = new DateOnly(2025, 2, 20), IsCredit = false }
        }.AsReadOnly();

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Groceries", Color = "#4CAF50" }
        }.AsReadOnly();

        _categoryRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(categories);

        var result = await _service.GetMonthlyComparisonAsync("2025-04");

        result.Should().ContainSingle();
        var comparison = result[0];
        comparison.CategoryId.Should().Be(1);
        comparison.CategoryName.Should().Be("Groceries");
        comparison.CurrentMonth.Should().Be(200m);
        comparison.PreviousMonth.Should().Be(150m);
        // 3-month window: Jan(100) + Feb(120) + Mar(150) = 370; avg = 370/3
        comparison.ThreeMonthAverage.Should().BeApproximately(123.33m, 0.01m);
    }

    [Fact]
    public async Task GetMonthlyComparisonAsync_NoPreviousData_PreviousMonthIsZero()
    {
        var transactions = new List<Transaction>
        {
            new() { Id = 1, CategoryId = 1, Amount = 200m, TransactionDate = new DateOnly(2025, 1, 5), IsCredit = false }
        }.AsReadOnly();

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        _categoryRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Category>
            {
                new() { Id = 1, Name = "Groceries", Color = "#4CAF50" }
            }.AsReadOnly());

        var result = await _service.GetMonthlyComparisonAsync("2025-01");

        result.Should().ContainSingle();
        result[0].CurrentMonth.Should().Be(200m);
        result[0].PreviousMonth.Should().Be(0m);
        result[0].ThreeMonthAverage.Should().Be(0m);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_NoTransactions_ReturnsZeroTotals()
    {
        SetupDefaultTransactionRepo(
            new List<Transaction>().AsReadOnly(),
            new List<Transaction>().AsReadOnly());

        _incomeRepo.FindAsync(Arg.Any<Expression<Func<Income, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Income>().AsReadOnly());

        var result = await _service.GetMonthlySummaryAsync("2025-06");

        result.Month.Should().Be("2025-06");
        result.TotalExpenses.Should().Be(0m);
        result.TotalIncome.Should().Be(0m);
        result.NetSavings.Should().Be(0m);
    }

    [Fact]
    public async Task GetCategoryBreakdownAsync_OrderedByTotalAmountDescending()
    {
        var transactions = new List<Transaction>
        {
            new() { Id = 1, CategoryId = 1, Amount = 50m, TransactionDate = new DateOnly(2025, 1, 5), IsCredit = false },
            new() { Id = 2, CategoryId = 2, Amount = 200m, TransactionDate = new DateOnly(2025, 1, 10), IsCredit = false },
            new() { Id = 3, CategoryId = 3, Amount = 100m, TransactionDate = new DateOnly(2025, 1, 15), IsCredit = false }
        }.AsReadOnly();

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        _categoryRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Category>
            {
                new() { Id = 1, Name = "A", Color = "#000" },
                new() { Id = 2, Name = "B", Color = "#111" },
                new() { Id = 3, Name = "C", Color = "#222" }
            }.AsReadOnly());

        var result = await _service.GetCategoryBreakdownAsync("2025-01");

        result[0].TotalAmount.Should().Be(200m);
        result[1].TotalAmount.Should().Be(100m);
        result[2].TotalAmount.Should().Be(50m);
    }
}
