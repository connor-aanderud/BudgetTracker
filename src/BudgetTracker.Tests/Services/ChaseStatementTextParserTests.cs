using BudgetTracker.Application.Services.Parsing;

namespace BudgetTracker.Tests.Services;

public class ChaseStatementTextParserTests
{
    // Mirrors the line layout PdfPig extracts from a real Chase Prime Visa statement
    // (Opening/Closing 01/29/26 - 02/28/26): a payments section and a purchase section,
    // followed by the totals/interest/points tail that must NOT be parsed as transactions.
    private static List<string> FebruaryStatementLines() =>
    [
        "CONNOR M AANDERUD",
        "Opening/Closing Date 01/29/26 - 02/28/26",
        "ACCOUNT ACTIVITY",
        "Date of",
        "Transaction",
        "Merchant Name or Transaction Description $ Amount",
        "PAYMENTS AND OTHER CREDITS",
        "02/19 Payment Thank You-Mobile -400.00",
        "02/25 Payment Thank You-Mobile -449.05",
        "PURCHASE",
        "01/28 WALMART.COM 800-925-6278 AR 77.46",
        "01/28 CENEX-PETRO SERVE USA #6 HARWOOD ND 40.37",
        "02/02 AMAZON MKTPL*JR3EE7W73 Amzn.com/bill WA 34.37",
        "Order Number 111-1881581-1004219",
        "02/16 HALSTAD TELEPHONE COMPAN 218-456-2125 MN 65.00",
        "2026 Totals Year-to-Date",
        "Total fees charged in 2026 $0.00",
        "INTEREST CHARGES",
        "SHOP WITH POINTS ACTIVITY",
        "02/16 AMAZON MARKETPLACE AMZN.COM/BILLWA 18.20 1,820",
    ];

    [Fact]
    public void Parse_RealStatementLayout_ReturnsOnlyAccountActivityRows()
    {
        var result = ChaseStatementTextParser.Parse(FebruaryStatementLines(), "list.pdf");

        result.Source.Should().Be("Chase Prime Visa");
        // 2 payments + 4 purchases. The SHOP WITH POINTS row is excluded.
        result.ParsedTransactions.Should().HaveCount(6);
        result.TotalCount.Should().Be(6);
        result.ParsedTransactions.Should().NotContain(t => t.RawDescription.Contains("AMAZON MARKETPLACE"));
    }

    [Fact]
    public void Parse_Payment_IsCreditFromNegativeAmount()
    {
        var result = ChaseStatementTextParser.Parse(FebruaryStatementLines(), "s.pdf");

        var payment = result.ParsedTransactions[0];
        payment.RawDescription.Should().Be("Payment Thank You-Mobile");
        payment.Amount.Should().Be(400.00m);
        payment.IsCredit.Should().BeTrue();
        payment.TransactionDate.Should().Be(new DateOnly(2026, 2, 19));
    }

    [Fact]
    public void Parse_Purchase_IsDebitWithInferredYear()
    {
        var result = ChaseStatementTextParser.Parse(FebruaryStatementLines(), "s.pdf");

        var walmart = result.ParsedTransactions.Single(t => t.RawDescription.StartsWith("WALMART.COM"));
        walmart.Amount.Should().Be(77.46m);
        walmart.IsCredit.Should().BeFalse();
        // January row on a February statement → same year.
        walmart.TransactionDate.Should().Be(new DateOnly(2026, 1, 28));
        walmart.PostDate.Should().BeNull();
    }

    [Fact]
    public void Parse_SkipsOrderNumberLines()
    {
        var result = ChaseStatementTextParser.Parse(FebruaryStatementLines(), "s.pdf");

        var amazon = result.ParsedTransactions.Single(t => t.RawDescription.StartsWith("AMAZON MKTPL"));
        amazon.RawDescription.Should().Be("AMAZON MKTPL*JR3EE7W73 Amzn.com/bill WA");
        amazon.RawDescription.Should().NotContain("Order Number");
    }

    [Fact]
    public void Parse_CommaAmount_ParsedCorrectly()
    {
        var lines = new List<string>
        {
            "Opening/Closing Date 03/29/26 - 04/28/26",
            "ACCOUNT ACTIVITY",
            "PAYMENTS AND OTHER CREDITS",
            "04/22 Payment Thank You-Mobile -1,489.88",
            "2026 Totals Year-to-Date",
        };

        var result = ChaseStatementTextParser.Parse(lines, "s.pdf");

        var payment = result.ParsedTransactions.Single();
        payment.Amount.Should().Be(1489.88m);
        payment.IsCredit.Should().BeTrue();
    }

    [Fact]
    public void Parse_AmountWithoutLeadingZero_ParsedCorrectly()
    {
        var lines = new List<string>
        {
            "Opening/Closing Date 03/29/26 - 04/28/26",
            "ACCOUNT ACTIVITY",
            "PURCHASE",
            "04/02 LINSON PHARMACY FARGO ND .55",
            "2026 Totals Year-to-Date",
        };

        var result = ChaseStatementTextParser.Parse(lines, "s.pdf");

        var tx = result.ParsedTransactions.Single();
        tx.RawDescription.Should().Be("LINSON PHARMACY FARGO ND");
        tx.Amount.Should().Be(0.55m);
        tx.IsCredit.Should().BeFalse();
    }

    [Fact]
    public void Parse_DecemberRowOnJanuaryStatement_UsesPreviousYear()
    {
        var lines = new List<string>
        {
            "Opening/Closing Date 12/29/25 - 01/28/26",
            "ACCOUNT ACTIVITY",
            "PURCHASE",
            "12/30 CASEYS #3296 FARGO ND 15.30",
            "01/05 WALMART.COM 800-925-6278 AR 20.00",
            "2026 Totals Year-to-Date",
        };

        var result = ChaseStatementTextParser.Parse(lines, "s.pdf");

        result.ParsedTransactions.Single(t => t.Amount == 15.30m)
            .TransactionDate.Should().Be(new DateOnly(2025, 12, 30));
        result.ParsedTransactions.Single(t => t.Amount == 20.00m)
            .TransactionDate.Should().Be(new DateOnly(2026, 1, 5));
    }

    [Fact]
    public void Parse_ExcludesFeesAndInterestSummary()
    {
        var result = ChaseStatementTextParser.Parse(FebruaryStatementLines(), "s.pdf");

        result.ParsedTransactions.Should().NotContain(t => t.RawDescription.Contains("fees"));
        result.ParsedTransactions.Should().NotContain(t => t.RawDescription.Contains("Total"));
    }

    [Fact]
    public void Parse_ComputesDuplicateHash()
    {
        var result = ChaseStatementTextParser.Parse(FebruaryStatementLines(), "s.pdf");

        result.ParsedTransactions.Should().OnlyContain(t => !string.IsNullOrEmpty(t.DuplicateHash));
    }
}
