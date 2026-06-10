using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using BudgetTracker.Application.DTOs;

namespace BudgetTracker.Application.Services.Parsing;

/// <summary>
/// Parses the plain-text lines extracted from an FNBO (First National Bank of Omaha)
/// SCHEELS Visa statement into transactions. Kept separate from the PDF extraction
/// (<see cref="FnboStatementParser"/>) so the line-parsing logic is unit-testable
/// without a real PDF.
/// </summary>
/// <remarks>
/// Statement layout (per real statements):
/// - Transactions live in two sections, each introduced by a column header containing
///   "Trans Date  Post Date  Reference Number  Transaction Description  Credits (CR) and Debits":
///   "Payments and Other Credits" (all rows carry a "CR" suffix) and "Transactions" (purchases).
/// - A transaction row looks like:
///     MM-DD  MM-DD  &lt;reference#&gt;  [mcc-digit]  &lt;description&gt;  $amount[ CR]
///   The description can wrap onto the following line(s), which have no date prefix.
/// - A trailing "CR" marks a credit (payment or refund). Refunds carry "CR" even inside
///   the purchases section, so credit detection is per-row, not per-section.
/// - Trans/Post dates are MM-DD with no year; the year is inferred from the statement
///   closing date ("billing cycle ending MM/DD/YYYY").
/// - Fees and interest appear only in the summary section, never as transaction rows.
/// </remarks>
public static class FnboStatementTextParser
{
    public const string SourceName = "FNBO";

    // MM-DD  MM-DD  <reference digits>  <rest...>
    private static readonly Regex TransactionLine = new(
        @"^(?<tm>\d{2})-(?<td>\d{2})\s+(?<pm>\d{2})-(?<pd>\d{2})\s+(?<ref>\d{6,})\b(?<rest>.*)$",
        RegexOptions.Compiled);

    // "For billing cycle ending 01/05/2026"  /  "...billing cycle ending 01/05/26"
    private static readonly Regex ClosingDate = new(
        @"billing cycle ending\s+(?<m>\d{2})/(?<d>\d{2})/(?<y>\d{2,4})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // A dollar amount, e.g. $1,218.78
    private static readonly Regex Amount = new(@"\$(?<amt>[\d,]+\.\d{2})", RegexOptions.Compiled);

    // A lone "MCC"-style digit the PDF places between the reference number and the merchant name.
    private static readonly Regex LeadingLoneDigit = new(@"^\d\s+", RegexOptions.Compiled);

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
            result.Warnings.Add("Could not find the statement closing date ('billing cycle ending'); " +
                                "transaction years may be inaccurate.");
        }

        var inSection = false;
        ParsedTransactionDto? current = null;
        var descriptionParts = new List<string>();

        void FinalizeCurrent()
        {
            if (current is null) return;
            current.RawDescription = NormalizeWhitespace(string.Join(" ", descriptionParts));
            current.DuplicateHash = ComputeHash(current.TransactionDate, current.Amount, current.RawDescription);
            result.ParsedTransactions.Add(current);
            current = null;
            descriptionParts = new List<string>();
        }

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;

            if (IsSectionHeader(line))
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

            // A sub-heading or wrapped column header ("Transactions", "Payments and Other Credits", "Debits")
            // closes the current row but keeps us in the transaction area.
            if (IsBoundary(line))
            {
                FinalizeCurrent();
                continue;
            }

            var match = TransactionLine.Match(line);
            if (match.Success)
            {
                FinalizeCurrent();
                current = BuildTransaction(match, closingDate, result.Warnings, out var firstDescPart);
                if (current is null) continue;
                if (!string.IsNullOrWhiteSpace(firstDescPart))
                    descriptionParts.Add(firstDescPart);
                continue;
            }

            // Otherwise it's a wrapped continuation of the current row's description.
            // Continuation lines never contain a dollar amount.
            if (current is not null && !line.Contains('$'))
                descriptionParts.Add(line);
        }

        FinalizeCurrent();

        result.TotalCount = result.ParsedTransactions.Count;
        return result;
    }

    private static ParsedTransactionDto? BuildTransaction(
        Match match, DateOnly? closingDate, List<string> warnings, out string description)
    {
        description = string.Empty;

        var rest = match.Groups["rest"].Value;
        var amountMatch = Amount.Match(rest);
        if (!amountMatch.Success)
        {
            // No amount on this row — not a real transaction line we can use.
            warnings.Add($"Skipped a row with no parsable amount: '{match.Value.Trim()}'");
            return null;
        }

        var amount = decimal.Parse(amountMatch.Groups["amt"].Value, NumberStyles.Number, CultureInfo.InvariantCulture);

        // Everything after the amount (e.g. " CR") tells us whether it's a credit.
        var afterAmount = rest[(amountMatch.Index + amountMatch.Length)..];
        var isCredit = Regex.IsMatch(afterAmount, @"\bCR\b", RegexOptions.IgnoreCase);

        // Description is the text between the reference number and the amount,
        // minus the stray leading MCC digit the PDF sometimes inserts.
        var descPart = rest[..amountMatch.Index];
        descPart = LeadingLoneDigit.Replace(descPart.TrimStart(), string.Empty);
        description = NormalizeWhitespace(descPart);

        var transDate = ResolveDate(
            int.Parse(match.Groups["tm"].Value), int.Parse(match.Groups["td"].Value), closingDate);
        var postDate = ResolveDate(
            int.Parse(match.Groups["pm"].Value), int.Parse(match.Groups["pd"].Value), closingDate);

        return new ParsedTransactionDto
        {
            TransactionDate = transDate,
            PostDate = postDate,
            Amount = amount,
            IsCredit = isCredit
        };
    }

    /// <summary>
    /// Statement rows show MM-DD with no year. Pick the year so the date falls on or before
    /// the closing date (December rows on a January statement belong to the previous year).
    /// </summary>
    private static DateOnly ResolveDate(int month, int day, DateOnly? closingDate)
    {
        var anchor = closingDate ?? DateOnly.FromDateTime(DateTime.Today);
        var candidate = SafeDate(anchor.Year, month, day);
        if (candidate > anchor)
            candidate = SafeDate(anchor.Year - 1, month, day);
        return candidate;
    }

    private static DateOnly SafeDate(int year, int month, int day)
    {
        // Guard against an impossible day for the month (e.g. truncated/garbled input).
        var clampedDay = Math.Min(day, DateTime.DaysInMonth(year, Math.Clamp(month, 1, 12)));
        return new DateOnly(year, Math.Clamp(month, 1, 12), Math.Max(clampedDay, 1));
    }

    private static DateOnly? FindClosingDate(IReadOnlyList<string> lines)
    {
        DateOnly? best = null;
        foreach (var line in lines)
        {
            var m = ClosingDate.Match(line);
            if (!m.Success) continue;

            var year = int.Parse(m.Groups["y"].Value);
            if (year < 100) year += 2000;
            var date = SafeDate(year, int.Parse(m.Groups["m"].Value), int.Parse(m.Groups["d"].Value));

            // Prefer a 4-digit year match if we find one.
            if (best is null || m.Groups["y"].Value.Length == 4)
                best = date;
        }
        return best;
    }

    private static bool IsSectionHeader(string line) =>
        line.Contains("Trans Date", StringComparison.OrdinalIgnoreCase) &&
        line.Contains("Post Date", StringComparison.OrdinalIgnoreCase);

    private static bool IsBoundary(string line) =>
        line.Equals("Transactions", StringComparison.OrdinalIgnoreCase) ||
        line.Equals("Debits", StringComparison.OrdinalIgnoreCase) ||
        line.StartsWith("Payments and Other Credits", StringComparison.OrdinalIgnoreCase) ||
        line.StartsWith("Credits (CR)", StringComparison.OrdinalIgnoreCase);

    private static bool IsSectionEnd(string line) =>
        line.StartsWith("Fees Charged", StringComparison.OrdinalIgnoreCase) ||
        line.StartsWith("Charge Summary", StringComparison.OrdinalIgnoreCase) ||
        line.StartsWith("Total Fees", StringComparison.OrdinalIgnoreCase) ||
        line.StartsWith("Contact Information", StringComparison.OrdinalIgnoreCase) ||
        line.Contains("REWARD YOUR PASSION", StringComparison.OrdinalIgnoreCase) ||
        line.Contains("Passion Points", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeWhitespace(string value) =>
        Regex.Replace(value, @"\s+", " ").Trim();

    private static string ComputeHash(DateOnly date, decimal amount, string description)
    {
        var input = $"{date:yyyy-MM-dd}|{amount:F2}|{description}|{SourceName}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
