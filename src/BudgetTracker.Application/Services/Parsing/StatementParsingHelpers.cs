using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BudgetTracker.Application.Services.Parsing;

/// <summary>
/// Shared helpers for bank-statement parsers (FNBO, Chase, ...). Statement rows
/// typically show a month/day with no year, so the year is inferred from the
/// statement's closing date.
/// </summary>
internal static class StatementParsingHelpers
{
    /// <summary>
    /// Resolves a month/day (no year) to a full date. The year is chosen so the date
    /// falls on or before the closing date — e.g. a December row on a January statement
    /// belongs to the previous year. Falls back to today's year if no closing date.
    /// </summary>
    public static DateOnly ResolveDate(int month, int day, DateOnly? closingDate)
    {
        var anchor = closingDate ?? DateOnly.FromDateTime(DateTime.Today);
        var candidate = SafeDate(anchor.Year, month, day);
        if (candidate > anchor)
            candidate = SafeDate(anchor.Year - 1, month, day);
        return candidate;
    }

    /// <summary>Builds a date, clamping an impossible month/day from garbled input.</summary>
    public static DateOnly SafeDate(int year, int month, int day)
    {
        var safeMonth = Math.Clamp(month, 1, 12);
        var safeDay = Math.Clamp(day, 1, DateTime.DaysInMonth(year, safeMonth));
        return new DateOnly(year, safeMonth, safeDay);
    }

    public static string NormalizeWhitespace(string value) =>
        Regex.Replace(value, @"\s+", " ").Trim();

    /// <summary>
    /// Stable hash used to detect duplicate transactions across re-imports.
    /// Matches the format used by the CSV parser so the same logical row hashes
    /// consistently within a source.
    /// </summary>
    public static string ComputeDuplicateHash(DateOnly date, decimal amount, string description, string source)
    {
        var input = $"{date:yyyy-MM-dd}|{amount:F2}|{description}|{source}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
