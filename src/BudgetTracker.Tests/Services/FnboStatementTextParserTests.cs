using BudgetTracker.Application.Services.Parsing;

namespace BudgetTracker.Tests.Services;

public class FnboStatementTextParserTests
{
    // Mirrors the line layout PdfPig extracts from a real FNBO SCHEELS Visa statement
    // (closing 01/05/2026): a payments section and a purchases section, with the
    // summary tail that must NOT be parsed as transactions.
    private static List<string> JanuaryStatementLines() =>
    [
        "CONNOR M AANDERUD",
        "Account number ending in 9690",
        "For billing cycle ending 01/05/2026",
        "TRANSACTION DETAIL",
        "Payments and Other Credits",
        "Trans Date Post Date Reference Number Transaction Description Credits (CR) and Debits",
        "12-23 12-23 74418005357045001307457 ONLINE PAYMENT THANK $1,218.78 CR",
        "YOU",
        "01-02 01-02 74418006002045001078840 ONLINE PAYMENT THANK $400.00 CR",
        "YOU",
        "Transactions",
        "Trans Date Post Date Reference Number Transaction Description Credits (CR) and",
        "Debits",
        "12-05 12-08 24793385339001182451225 Scheels Campus Fargo ND $59.26",
        "12-11 12-12 24431065345341707242297 7 TARGET.COM * 800-591-3869 $26.93",
        "MN",
        "12-16 12-17 24943005351344911515100 CENEX-PETRO SERVE USA #6 $34.90",
        "HARWOOD ND",
        "01-04 01-05 24793386003000027652081 5 Store Mayville ND $25.55",
        "Fees Charged Interest Charged",
        "Total Fees for this period $0.00 Interest Charge on Purchases $0.00",
        "Total Interest for this Period $0.00",
    ];

    [Fact]
    public void Parse_RealStatementLayout_ReturnsAllTransactions()
    {
        var result = FnboStatementTextParser.Parse(JanuaryStatementLines(), "2026-01-05.pdf");

        result.Source.Should().Be("FNBO");
        result.FileName.Should().Be("2026-01-05.pdf");
        result.ParsedTransactions.Should().HaveCount(6);
        result.TotalCount.Should().Be(6);
    }

    [Fact]
    public void Parse_Payment_IsCreditWithWrappedDescription()
    {
        var result = FnboStatementTextParser.Parse(JanuaryStatementLines(), "s.pdf");

        var payment = result.ParsedTransactions[0];
        payment.RawDescription.Should().Be("ONLINE PAYMENT THANK YOU");
        payment.Amount.Should().Be(1218.78m);
        payment.IsCredit.Should().BeTrue();
        // December row on a January statement → previous year.
        payment.TransactionDate.Should().Be(new DateOnly(2025, 12, 23));
    }

    [Fact]
    public void Parse_Purchase_IsDebitWithTransAndPostDates()
    {
        var result = FnboStatementTextParser.Parse(JanuaryStatementLines(), "s.pdf");

        var scheels = result.ParsedTransactions.Single(t => t.RawDescription == "Scheels Campus Fargo ND");
        scheels.Amount.Should().Be(59.26m);
        scheels.IsCredit.Should().BeFalse();
        scheels.TransactionDate.Should().Be(new DateOnly(2025, 12, 5));
        scheels.PostDate.Should().Be(new DateOnly(2025, 12, 8));
    }

    [Fact]
    public void Parse_StripsStrayMccDigitBeforeMerchantName()
    {
        var result = FnboStatementTextParser.Parse(JanuaryStatementLines(), "s.pdf");

        // "7 TARGET.COM..." → leading lone digit removed; wrapped "MN" appended.
        var target = result.ParsedTransactions.Single(t => t.Amount == 26.93m);
        target.RawDescription.Should().Be("TARGET.COM * 800-591-3869 MN");

        var store = result.ParsedTransactions.Single(t => t.Amount == 25.55m);
        store.RawDescription.Should().Be("Store Mayville ND");
        // January row on a January statement → same year.
        store.TransactionDate.Should().Be(new DateOnly(2026, 1, 4));
    }

    [Fact]
    public void Parse_MultiLineDescription_IsJoined()
    {
        var result = FnboStatementTextParser.Parse(JanuaryStatementLines(), "s.pdf");

        var cenex = result.ParsedTransactions.Single(t => t.Amount == 34.90m);
        cenex.RawDescription.Should().Be("CENEX-PETRO SERVE USA #6 HARWOOD ND");
        cenex.IsCredit.Should().BeFalse();
    }

    [Fact]
    public void Parse_DoesNotTreatFeesOrSummaryLinesAsTransactions()
    {
        var result = FnboStatementTextParser.Parse(JanuaryStatementLines(), "s.pdf");

        result.ParsedTransactions.Should().NotContain(t => t.RawDescription.Contains("Fees"));
        result.ParsedTransactions.Should().NotContain(t => t.RawDescription.Contains("Interest"));
        result.ParsedTransactions.Should().NotContain(t => t.RawDescription.Contains("Total"));
    }

    [Fact]
    public void Parse_RefundInPurchasesSection_IsCredit()
    {
        // A refund carries "CR" even though it sits in the Transactions (purchases) section.
        // Closing 05/06/2026 so the 04-29 row resolves to 2026.
        var lines = new List<string>
        {
            "For billing cycle ending 05/06/2026",
            "Transactions",
            "Trans Date Post Date Reference Number Transaction Description Credits (CR) and",
            "Debits",
            "04-29 04-29 74793386119000094317085 7 Scheels All Sports Inc $14.87 CR",
            "05-04 05-06 74692166125409608559142 INTERSTATE ALL BATTERY $16.16 CR",
            "Fees Charged Interest Charged",
        };

        var result = FnboStatementTextParser.Parse(lines, "2026-05-06.pdf");

        result.ParsedTransactions.Should().HaveCount(2);
        var refund = result.ParsedTransactions[0];
        refund.RawDescription.Should().Be("Scheels All Sports Inc");
        refund.Amount.Should().Be(14.87m);
        refund.IsCredit.Should().BeTrue();
        refund.TransactionDate.Should().Be(new DateOnly(2026, 4, 29));
    }

    [Fact]
    public void Parse_ComputesDuplicateHash()
    {
        var result = FnboStatementTextParser.Parse(JanuaryStatementLines(), "s.pdf");

        result.ParsedTransactions.Should().OnlyContain(t => !string.IsNullOrEmpty(t.DuplicateHash));
    }

    [Fact]
    public void Parse_MissingClosingDate_AddsWarning()
    {
        var lines = new List<string>
        {
            "Transactions",
            "Trans Date Post Date Reference Number Transaction Description Credits (CR) and Debits",
            "12-05 12-08 24793385339001182451225 Scheels Campus Fargo ND $59.26",
        };

        var result = FnboStatementTextParser.Parse(lines, "s.pdf");

        result.Warnings.Should().Contain(w => w.Contains("closing date"));
        result.ParsedTransactions.Should().ContainSingle();
    }

    [Fact]
    public void Parse_IgnoresContentOutsideTransactionSections()
    {
        var lines = new List<string>
        {
            "For billing cycle ending 01/05/2026",
            "New Balance $506.48 Minimum Payment $40.00",
            "Total Credit Limit $3,200.00",
            "02/03/2026 Payment Due Date",
        };

        var result = FnboStatementTextParser.Parse(lines, "s.pdf");

        result.ParsedTransactions.Should().BeEmpty();
    }
}
