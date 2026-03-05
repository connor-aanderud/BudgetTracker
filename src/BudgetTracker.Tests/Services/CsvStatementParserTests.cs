using System.Text;
using BudgetTracker.Application.Services.Parsing;

namespace BudgetTracker.Tests.Services;

public class CsvStatementParserTests
{
    private readonly CsvStatementParser _parser = new();

    private static MemoryStream CreateStream(string content)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return stream;
    }

    [Fact]
    public async Task ParseAsync_ValidChaseFormat_ReturnsCorrectTransactions()
    {
        var csv = """
            Transaction Date,Post Date,Description,Category,Type,Amount
            01/15/2025,01/16/2025,AMAZON.COM,Shopping,Sale,-52.99
            01/20/2025,01/21/2025,PAYMENT RECEIVED,,Payment,200.00
            """;

        using var stream = CreateStream(csv);
        var result = await _parser.ParseAsync(stream, "chase.csv");

        result.Source.Should().Be("Chase");
        result.ParsedTransactions.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);

        var debit = result.ParsedTransactions[0];
        debit.TransactionDate.Should().Be(new DateOnly(2025, 1, 15));
        debit.PostDate.Should().Be(new DateOnly(2025, 1, 16));
        debit.RawDescription.Should().Be("AMAZON.COM");
        debit.Amount.Should().Be(52.99m);
        debit.IsCredit.Should().BeFalse();

        var credit = result.ParsedTransactions[1];
        credit.Amount.Should().Be(200.00m);
        credit.IsCredit.Should().BeTrue();
    }

    [Fact]
    public async Task ParseAsync_ValidMintFormat_ReturnsCorrectTransactions()
    {
        var csv = """
            Date,Description,Original Description,Amount,Transaction Type,Category,Account Name,Labels,Notes
            01/10/2025,Starbucks,STARBUCKS STORE 1234,5.75,debit,Coffee,Chase Visa,,
            01/12/2025,Paycheck,EMPLOYER DIRECT DEP,3000.00,credit,Income,Chase Checking,,
            """;

        using var stream = CreateStream(csv);
        var result = await _parser.ParseAsync(stream, "mint.csv");

        result.Source.Should().Be("Mint");
        result.ParsedTransactions.Should().HaveCount(2);

        var debit = result.ParsedTransactions[0];
        debit.TransactionDate.Should().Be(new DateOnly(2025, 1, 10));
        debit.RawDescription.Should().Be("STARBUCKS STORE 1234");
        debit.Amount.Should().Be(5.75m);
        debit.IsCredit.Should().BeFalse();

        var credit = result.ParsedTransactions[1];
        credit.Amount.Should().Be(3000.00m);
        credit.IsCredit.Should().BeTrue();
    }

    [Fact]
    public async Task ParseAsync_GenericFormat_ReturnsTransactions()
    {
        var csv = """
            Date,Description,Amount
            2025-01-05,Grocery Store,-85.50
            2025-01-06,Refund,25.00
            """;

        using var stream = CreateStream(csv);
        var result = await _parser.ParseAsync(stream, "transactions.csv");

        result.Source.Should().Be("Generic");
        result.ParsedTransactions.Should().HaveCount(2);

        var debit = result.ParsedTransactions[0];
        debit.TransactionDate.Should().Be(new DateOnly(2025, 1, 5));
        debit.RawDescription.Should().Be("Grocery Store");
        debit.Amount.Should().Be(85.50m);
        debit.IsCredit.Should().BeFalse();

        var credit = result.ParsedTransactions[1];
        credit.Amount.Should().Be(25.00m);
        credit.IsCredit.Should().BeTrue();
    }

    [Fact]
    public async Task ParseAsync_EmptyCsv_ReturnsEmptyResultWithWarning()
    {
        var csv = "";

        using var stream = CreateStream(csv);
        var result = await _parser.ParseAsync(stream, "empty.csv");

        result.ParsedTransactions.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Warnings.Should().ContainSingle()
            .Which.Should().Contain("empty");
    }

    [Fact]
    public async Task ParseAsync_HeaderOnlyNoCsvData_ReturnsEmptyTransactions()
    {
        var csv = "Date,Description,Amount\n";

        using var stream = CreateStream(csv);
        var result = await _parser.ParseAsync(stream, "headeronly.csv");

        result.ParsedTransactions.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task ParseAsync_ChaseFormat_ComputesDuplicateHash()
    {
        var csv = """
            Transaction Date,Post Date,Description,Category,Type,Amount
            01/15/2025,01/16/2025,AMAZON.COM,Shopping,Sale,-52.99
            """;

        using var stream = CreateStream(csv);
        var result = await _parser.ParseAsync(stream, "chase.csv");

        result.ParsedTransactions.Should().ContainSingle();
        result.ParsedTransactions[0].DuplicateHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ParseAsync_GenericFormatWithSeparateDebitCreditColumns_ParsesCorrectly()
    {
        var csv = """
            Date,Description,Debit,Credit
            2025-01-05,Grocery Store,85.50,
            2025-01-06,Refund,,25.00
            """;

        using var stream = CreateStream(csv);
        var result = await _parser.ParseAsync(stream, "bank.csv");

        result.Source.Should().Be("Generic");
        result.ParsedTransactions.Should().HaveCount(2);

        var debit = result.ParsedTransactions[0];
        debit.Amount.Should().Be(85.50m);
        debit.IsCredit.Should().BeFalse();

        var credit = result.ParsedTransactions[1];
        credit.Amount.Should().Be(25.00m);
        credit.IsCredit.Should().BeTrue();
    }
}
