using System.Globalization;
using System.Text.RegularExpressions;
using BudgetTracker.Application.DTOs;

namespace BudgetTracker.Application.Services.Parsing;

/// <summary>
/// Parses the plain-text lines extracted from a Chase Prime Visa (Amazon) statement
/// into transactions. Kept separate from the PDF extraction so the line-parsing logic
/// is unit-testable without a real PDF.
/// </summary>
/// <remarks>
/// Statement layout (per real statements):
/// - Transactions live under the "ACCOUNT ACTIVITY" heading, in two sub-sections:
///   "PAYMENTS AND OTHER CREDITS" and "PURCHASE".
/// - A transaction row looks like:  MM/DD  &lt;description&gt;  &lt;amount&gt;
///   where the amount is the trailing number. A NEGATIVE amount marks a credit
///   (payment or refund); positive is a purchase. Amounts may carry thousands commas
///   and may omit the leading zero (e.g. ".55").
/// - The date is MM/DD with no year; the year is inferred from the statement closing date
///   ("Opening/Closing Date MM/DD/YY - MM/DD/YY" or "Statement Date: MM/DD/YY").
/// - "Order Number ..." lines are metadata, not part of the description.
/// - The "SHOP WITH POINTS ACTIVITY" / "2026 Totals" / "INTEREST CHARGES" sections that
///   follow are NOT card charges and must be excluded.
/// </remarks>
public static class ChaseStatementTextParser
{
    public const string SourceName = "Chase Prime Visa";

    // MM/DD  <description>  <trailing amount>
    private static readonly Regex TransactionLine = new(
        @"^(?<m>\d{2})/(?<d>\d{2})\s+(?<desc>.+?)\s+(?<amt>-?(?:\d[\d,]*)?\.\d{2})$",
        RegexOptions.Compiled);

    // "Opening/Closing Date 01/29/26 - 02/28/26"  (closing = the second date)
    private static readonly Regex OpeningClosingDate = new(
        @"Opening/Closing Date\s+\d{2}/\d{2}/\d{2}\s*-\s*(?<m>\d{2})/(?<d>\d{2})/(?<y>\d{2})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // "Statement Date: 02/28/26"  (fallback)
    private static readonly Regex StatementDate = new(
        @"Statement Date:\s*(?<m>\d{2})/(?<d>\d{2})/(?<y>\d{2})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static ParseResultDto Parse(IReadOnlyList<string> lines, string fileName)
    {
        var result = new ParseResultDto
        {
            FileName = fileName,
            Source = SourceName,
            Warnings = new List<string>(),
            ParsedTransactions = new List<ParsedTransactionDto>()
        };

        var closingDate = FindClosingDate(lines);
        if (closingDate is null)
        {
            result.Warnings.Add("Could not find the statement closing date ('Opening/Closing Date'); " +
                                "transaction years may be inaccurate.");
        }

        var inSection = false;
        ParsedTransactionDto? current = null;
        var descriptionParts = new List<string>();

        void FinalizeCurrent()
        {
            if (current is null) return;
            current.RawDescription = StatementParsingHelpers.NormalizeWhitespace(string.Join(" ", descriptionParts));
            current.DuplicateHash = StatementParsingHelpers.ComputeDuplicateHash(
                current.TransactionDate, current.Amount, current.RawDescription, SourceName);
            result.ParsedTransactions.Add(current);
            current = null;
            descriptionParts = new List<string>();
        }

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;

            if (IsSectionStart(line))
            {
                FinalizeCurrent();
                inSection = true;
                continue;
            }

            if (IsSectionEnd(line))
            {
                FinalizeCurrent();
                inSection = false;
                continue;
            }

            if (!inSection) continue;

            if (IsBoundary(line))
            {
                FinalizeCurrent();
                continue;
            }

            var match = TransactionLine.Match(line);
            if (match.Success)
            {
                FinalizeCurrent();
                current = BuildTransaction(match, closingDate);
                descriptionParts.Add(match.Groups["desc"].Value);
                continue;
            }

            // "Order Number ..." lines are metadata, not part of the merchant description.
            if (line.StartsWith("Order Number", StringComparison.OrdinalIgnoreCase)) continue;

            // Otherwise a wrapped continuation of the current row's description.
            if (current is not null)
                descriptionParts.Add(line);
        }

        FinalizeCurrent();

        result.TotalCount = result.ParsedTransactions.Count;
        return result;
    }

    private static ParsedTransactionDto BuildTransaction(Match match, DateOnly? closingDate)
    {
        var value = ParseAmount(match.Groups["amt"].Value);

        return new ParsedTransactionDto
        {
            TransactionDate = StatementParsingHelpers.ResolveDate(
                int.Parse(match.Groups["m"].Value), int.Parse(match.Groups["d"].Value), closingDate),
            // Chase activity rows show a single transaction date, no separate post date.
            PostDate = null,
            Amount = Math.Abs(value),
            IsCredit = value < 0
        };
    }

    private static decimal ParseAmount(string raw)
    {
        var cleaned = raw.Replace(",", "");
        // Restore a leading zero for amounts like ".55" or "-.55".
        if (cleaned.StartsWith('.')) cleaned = "0" + cleaned;
        else if (cleaned.StartsWith("-.")) cleaned = "-0" + cleaned[1..];
        return decimal.Parse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture);
    }

    private static DateOnly? FindClosingDate(IReadOnlyList<string> lines)
    {
        foreach (var line in lines)
        {
            var m = OpeningClosingDate.Match(line);
            if (m.Success)
                return BuildClosingDate(m);
        }

        foreach (var line in lines)
        {
            var m = StatementDate.Match(line);
            if (m.Success)
                return BuildClosingDate(m);
        }

        return null;
    }

    private static DateOnly BuildClosingDate(Match m)
    {
        var year = int.Parse(m.Groups["y"].Value);
        if (year < 100) year += 2000;
        return StatementParsingHelpers.SafeDate(year, int.Parse(m.Groups["m"].Value), int.Parse(m.Groups["d"].Value));
    }

    private static bool IsSectionStart(string line) =>
        line.Contains("ACCOUNT ACTIVITY", StringComparison.OrdinalIgnoreCase);

    private static bool IsBoundary(string line) =>
        line.Equals("PURCHASE", StringComparison.OrdinalIgnoreCase) ||
        line.StartsWith("PAYMENTS AND OTHER CREDITS", StringComparison.OrdinalIgnoreCase) ||
        line.Contains("Merchant Name or Transaction Description", StringComparison.OrdinalIgnoreCase) ||
        line.Equals("Date of", StringComparison.OrdinalIgnoreCase) ||
        line.Equals("Transaction", StringComparison.OrdinalIgnoreCase) ||
        line.Equals("$ Amount", StringComparison.OrdinalIgnoreCase);

    private static bool IsSectionEnd(string line) =>
        line.StartsWith("2026 Totals", StringComparison.OrdinalIgnoreCase) ||
        line.Contains("Totals Year-to-Date", StringComparison.OrdinalIgnoreCase) ||
        line.StartsWith("Total fees charged", StringComparison.OrdinalIgnoreCase) ||
        line.StartsWith("Total interest charged", StringComparison.OrdinalIgnoreCase) ||
        line.Contains("INTEREST CHARGES", StringComparison.OrdinalIgnoreCase) ||
        line.Contains("Days in Billing Period", StringComparison.OrdinalIgnoreCase);
}
