using System.Linq.Expressions;
using BudgetTracker.Application.Interfaces;
using BudgetTracker.Application.Services;
using BudgetTracker.Domain.Entities;
using NSubstitute;

namespace BudgetTracker.Tests.Services;

public class BudgetServiceTests
{
    private readonly IBudgetRepository _budgetRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly BudgetService _service;

    public BudgetServiceTests()
    {
        _budgetRepo = Substitute.For<IBudgetRepository>();
        _transactionRepo = Substitute.For<ITransactionRepository>();
        _categoryRepo = Substitute.For<ICategoryRepository>();
        _service = new BudgetService(_budgetRepo, _transactionRepo, _categoryRepo);
    }

    [Fact]
    public async Task GetBudgetStatusAsync_WithTransactions_ComputesActualsCorrectly()
    {
        var groceriesCategory = new Category { Id = 1, Name = "Groceries", Color = "#4CAF50" };
        var budgets = new List<Budget>
        {
            new() { Id = 1, CategoryId = 1, Month = "2025-01", LimitAmount = 500m, Category = groceriesCategory }
        }.AsReadOnly();

        _budgetRepo.GetByMonthAsync("2025-01", Arg.Any<CancellationToken>())
            .Returns(budgets);

        var transactions = new List<Transaction>
        {
            new() { Id = 1, CategoryId = 1, Amount = 50m, TransactionDate = new DateOnly(2025, 1, 5), IsCredit = false },
            new() { Id = 2, CategoryId = 1, Amount = 75m, TransactionDate = new DateOnly(2025, 1, 10), IsCredit = false },
            new() { Id = 3, CategoryId = 1, Amount = 25m, TransactionDate = new DateOnly(2025, 1, 15), IsCredit = false }
        }.AsReadOnly();

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        var result = await _service.GetBudgetStatusAsync("2025-01");

        result.Should().ContainSingle();
        var status = result[0];
        status.CategoryId.Should().Be(1);
        status.CategoryName.Should().Be("Groceries");
        status.LimitAmount.Should().Be(500m);
        status.ActualAmount.Should().Be(150m);
        status.RemainingAmount.Should().Be(350m);
        status.OverBudget.Should().BeFalse();
    }

    [Fact]
    public async Task GetBudgetStatusAsync_OverBudget_ReturnsOverBudgetTrue()
    {
        var diningCategory = new Category { Id = 2, Name = "Dining", Color = "#FF5722" };
        var budgets = new List<Budget>
        {
            new() { Id = 1, CategoryId = 2, Month = "2025-01", LimitAmount = 100m, Category = diningCategory }
        }.AsReadOnly();

        _budgetRepo.GetByMonthAsync("2025-01", Arg.Any<CancellationToken>())
            .Returns(budgets);

        var transactions = new List<Transaction>
        {
            new() { Id = 1, CategoryId = 2, Amount = 60m, TransactionDate = new DateOnly(2025, 1, 5), IsCredit = false },
            new() { Id = 2, CategoryId = 2, Amount = 65m, TransactionDate = new DateOnly(2025, 1, 15), IsCredit = false }
        }.AsReadOnly();

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        var result = await _service.GetBudgetStatusAsync("2025-01");

        var status = result[0];
        status.ActualAmount.Should().Be(125m);
        status.RemainingAmount.Should().Be(-25m);
        status.OverBudget.Should().BeTrue();
    }

    [Fact]
    public async Task GetBudgetStatusAsync_UnderBudget_ReturnsCorrectRemaining()
    {
        var category = new Category { Id = 3, Name = "Entertainment", Color = "#9C27B0" };
        var budgets = new List<Budget>
        {
            new() { Id = 1, CategoryId = 3, Month = "2025-02", LimitAmount = 200m, Category = category }
        }.AsReadOnly();

        _budgetRepo.GetByMonthAsync("2025-02", Arg.Any<CancellationToken>())
            .Returns(budgets);

        var transactions = new List<Transaction>
        {
            new() { Id = 1, CategoryId = 3, Amount = 30m, TransactionDate = new DateOnly(2025, 2, 10), IsCredit = false }
        }.AsReadOnly();

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        var result = await _service.GetBudgetStatusAsync("2025-02");

        var status = result[0];
        status.RemainingAmount.Should().Be(170m);
        status.OverBudget.Should().BeFalse();
    }

    [Fact]
    public async Task GetBudgetStatusAsync_PercentUsedCalculatedCorrectly()
    {
        var category = new Category { Id = 1, Name = "Groceries", Color = "#4CAF50" };
        var budgets = new List<Budget>
        {
            new() { Id = 1, CategoryId = 1, Month = "2025-01", LimitAmount = 200m, Category = category }
        }.AsReadOnly();

        _budgetRepo.GetByMonthAsync("2025-01", Arg.Any<CancellationToken>())
            .Returns(budgets);

        var transactions = new List<Transaction>
        {
            new() { Id = 1, CategoryId = 1, Amount = 100m, TransactionDate = new DateOnly(2025, 1, 10), IsCredit = false }
        }.AsReadOnly();

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        var result = await _service.GetBudgetStatusAsync("2025-01");

        result[0].PercentUsed.Should().Be(50.0);
    }

    [Fact]
    public async Task GetBudgetStatusAsync_ZeroLimitAmount_PercentUsedIsZero()
    {
        var category = new Category { Id = 1, Name = "Misc", Color = "#607D8B" };
        var budgets = new List<Budget>
        {
            new() { Id = 1, CategoryId = 1, Month = "2025-01", LimitAmount = 0m, Category = category }
        }.AsReadOnly();

        _budgetRepo.GetByMonthAsync("2025-01", Arg.Any<CancellationToken>())
            .Returns(budgets);

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Transaction>().AsReadOnly());

        var result = await _service.GetBudgetStatusAsync("2025-01");

        result[0].PercentUsed.Should().Be(0);
    }

    [Fact]
    public async Task GetBudgetStatusAsync_EmptyMonth_ReturnsEmptyResult()
    {
        _budgetRepo.GetByMonthAsync("2025-03", Arg.Any<CancellationToken>())
            .Returns(new List<Budget>().AsReadOnly());

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Transaction>().AsReadOnly());

        var result = await _service.GetBudgetStatusAsync("2025-03");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBudgetStatusAsync_NoTransactionsForCategory_ActualIsZero()
    {
        var category = new Category { Id = 5, Name = "Travel", Color = "#00BCD4" };
        var budgets = new List<Budget>
        {
            new() { Id = 1, CategoryId = 5, Month = "2025-01", LimitAmount = 1000m, Category = category }
        }.AsReadOnly();

        _budgetRepo.GetByMonthAsync("2025-01", Arg.Any<CancellationToken>())
            .Returns(budgets);

        // Return transactions for a different category
        var transactions = new List<Transaction>
        {
            new() { Id = 1, CategoryId = 2, Amount = 50m, TransactionDate = new DateOnly(2025, 1, 5), IsCredit = false }
        }.AsReadOnly();

        _transactionRepo.FindAsync(Arg.Any<Expression<Func<Transaction, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(transactions);

        var result = await _service.GetBudgetStatusAsync("2025-01");

        result[0].ActualAmount.Should().Be(0m);
        result[0].RemainingAmount.Should().Be(1000m);
    }

    [Fact]
    public async Task CopyBudgetsAsync_DelegatesToRepository()
    {
        await _service.CopyBudgetsAsync("2025-01", "2025-02");

        await _budgetRepo.Received(1)
            .CopyBudgetsAsync("2025-01", "2025-02", Arg.Any<CancellationToken>());
    }
}
