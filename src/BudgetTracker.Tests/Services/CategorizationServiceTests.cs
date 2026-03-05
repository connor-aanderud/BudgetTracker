using BudgetTracker.Application.Interfaces;
using BudgetTracker.Application.Services;
using BudgetTracker.Domain.Entities;
using NSubstitute;

namespace BudgetTracker.Tests.Services;

public class CategorizationServiceTests
{
    private readonly IMerchantRepository _merchantRepo;
    private readonly CategorizationService _service;

    public CategorizationServiceTests()
    {
        _merchantRepo = Substitute.For<IMerchantRepository>();
        _service = new CategorizationService(_merchantRepo);
    }

    [Fact]
    public async Task CategorizeAsync_MatchingMerchant_AssignsCategoryAndMerchant()
    {
        var merchant = new Merchant
        {
            Id = 10,
            NormalizedName = "Amazon",
            CategoryId = 5
        };

        _merchantRepo.FindMatchingMerchantAsync("AMAZON.COM", Arg.Any<CancellationToken>())
            .Returns(merchant);

        var transactions = new List<Transaction>
        {
            new()
            {
                Id = 1,
                RawDescription = "AMAZON.COM",
                Amount = 50m,
                TransactionDate = new DateOnly(2025, 1, 15)
            }
        };

        await _service.CategorizeAsync(transactions);

        transactions[0].MerchantId.Should().Be(10);
        transactions[0].CategoryId.Should().Be(5);
    }

    [Fact]
    public async Task CategorizeAsync_NoMatchingMerchant_LeavesTransactionUncategorized()
    {
        _merchantRepo.FindMatchingMerchantAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Merchant?)null);

        var transactions = new List<Transaction>
        {
            new()
            {
                Id = 1,
                RawDescription = "UNKNOWN VENDOR",
                Amount = 25m,
                TransactionDate = new DateOnly(2025, 1, 15)
            }
        };

        await _service.CategorizeAsync(transactions);

        transactions[0].MerchantId.Should().BeNull();
        transactions[0].CategoryId.Should().BeNull();
    }

    [Fact]
    public async Task CategorizeAsync_ManuallyRecategorized_SkipsTransaction()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                Id = 1,
                RawDescription = "AMAZON.COM",
                Amount = 50m,
                TransactionDate = new DateOnly(2025, 1, 15),
                ManuallyRecategorized = true,
                CategoryId = 99,
                MerchantId = 88
            }
        };

        await _service.CategorizeAsync(transactions);

        transactions[0].CategoryId.Should().Be(99);
        transactions[0].MerchantId.Should().Be(88);
        await _merchantRepo.DidNotReceive()
            .FindMatchingMerchantAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CategorizeAsync_MixedTransactions_OnlyCategorizesNonManual()
    {
        var merchant = new Merchant
        {
            Id = 10,
            NormalizedName = "Starbucks",
            CategoryId = 3
        };

        _merchantRepo.FindMatchingMerchantAsync("STARBUCKS STORE", Arg.Any<CancellationToken>())
            .Returns(merchant);
        _merchantRepo.FindMatchingMerchantAsync("SOME OTHER STORE", Arg.Any<CancellationToken>())
            .Returns((Merchant?)null);

        var transactions = new List<Transaction>
        {
            new()
            {
                Id = 1,
                RawDescription = "STARBUCKS STORE",
                Amount = 5m,
                TransactionDate = new DateOnly(2025, 1, 10)
            },
            new()
            {
                Id = 2,
                RawDescription = "SOME OTHER STORE",
                Amount = 20m,
                TransactionDate = new DateOnly(2025, 1, 11),
                ManuallyRecategorized = true,
                CategoryId = 7
            }
        };

        await _service.CategorizeAsync(transactions);

        transactions[0].MerchantId.Should().Be(10);
        transactions[0].CategoryId.Should().Be(3);

        transactions[1].CategoryId.Should().Be(7);
        transactions[1].MerchantId.Should().BeNull();
    }

    [Fact]
    public async Task CategorizeAsync_EmptyList_CompletesWithoutError()
    {
        var transactions = new List<Transaction>();

        await _service.CategorizeAsync(transactions);

        await _merchantRepo.DidNotReceive()
            .FindMatchingMerchantAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CategorizeAsync_MultipleMatches_EachGetsCorrectMerchant()
    {
        var amazonMerchant = new Merchant { Id = 10, NormalizedName = "Amazon", CategoryId = 5 };
        var starbucksMerchant = new Merchant { Id = 20, NormalizedName = "Starbucks", CategoryId = 3 };

        _merchantRepo.FindMatchingMerchantAsync("AMAZON.COM", Arg.Any<CancellationToken>())
            .Returns(amazonMerchant);
        _merchantRepo.FindMatchingMerchantAsync("STARBUCKS STORE", Arg.Any<CancellationToken>())
            .Returns(starbucksMerchant);

        var transactions = new List<Transaction>
        {
            new()
            {
                Id = 1,
                RawDescription = "AMAZON.COM",
                Amount = 50m,
                TransactionDate = new DateOnly(2025, 1, 15)
            },
            new()
            {
                Id = 2,
                RawDescription = "STARBUCKS STORE",
                Amount = 5m,
                TransactionDate = new DateOnly(2025, 1, 16)
            }
        };

        await _service.CategorizeAsync(transactions);

        transactions[0].MerchantId.Should().Be(10);
        transactions[0].CategoryId.Should().Be(5);
        transactions[1].MerchantId.Should().Be(20);
        transactions[1].CategoryId.Should().Be(3);
    }
}
