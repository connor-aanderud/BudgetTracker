using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;

namespace BudgetTracker.Application.Services.Parsing;

public class CsvStatementParser : IStatementParser
{
    public async Task<ParseResultDto> ParseAsync(Stream fileStream, string fileName)
    {
        var result = new ParseResultDto
        {
            FileName = fileName,
            Warnings = new List<string>(),
            ParsedTransactions = new List<ParsedTransactionDto>()
        };

        using var reader = new StreamReader(fileStream);
        var headerLine = await reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            result.Warnings.Add("File is empty or has no header row.");
            return result;
        }

        var format = DetectFormat(headerLine);
        result.Source = format.SourceName;

        // Reset stream to beginning for CsvHelper
        fileStream.Position = 0;

        using var csvReader = new CsvReader(new StreamReader(fileStream), new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = context => result.Warnings.Add($"Bad data at row {context.Context.Parser.Row}: {context.Field}")
        });

        csvReader.Read();
        csvReader.ReadHeader();

        var rowNumber = 1;
        while (csvReader.Read())
        {
            rowNumber++;
            try
            {
                var transaction = format.ParseRow(csvReader, result.Warnings, rowNumber);
                if (transaction != null)
                {
                    transaction.DuplicateHash = ComputeHash(
                        transaction.TransactionDate,
                        transaction.Amount,
                        transaction.RawDescription,
                        format.SourceName ?? "Unknown");
                    result.ParsedTransactions.Add(transaction);
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Row {rowNumber}: {ex.Message}");
            }
        }

        result.TotalCount = result.ParsedTransactions.Count;
        return result;
    }

    private static CsvFormat DetectFormat(string headerLine)
    {
        var header = headerLine.ToLowerInvariant();

        if (header.Contains("post date") && header.Contains("type") && header.Contains("amount") && header.Contains("description"))
            return new ChaseFormat();

        if (header.Contains("original description") && header.Contains("transaction type") && header.Contains("account name"))
            return new MintFormat();

        return new GenericFormat();
    }

    private static string ComputeHash(DateOnly date, decimal amount, string description, string source)
    {
        var input = $"{date:yyyy-MM-dd}|{amount:F2}|{description}|{source}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}

internal abstract class CsvFormat
{
    public abstract string? SourceName { get; }
    public abstract ParsedTransactionDto? ParseRow(CsvReader csv, List<string> warnings, int rowNumber);

    protected static DateOnly ParseDate(string? value, int rowNumber, List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new FormatException("Missing date value");

        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        // Try common US formats
        string[] formats = ["MM/dd/yyyy", "M/d/yyyy", "MM-dd-yyyy", "yyyy-MM-dd", "M/d/yy", "MM/dd/yy"];
        if (DateOnly.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            return date;

        throw new FormatException($"Cannot parse date: '{value}'");
    }

    protected static decimal ParseAmount(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        // Remove currency symbols and whitespace
        var cleaned = value.Replace("$", "").Replace(",", "").Trim();

        // Handle parentheses as negative: (100.00) → -100.00
        if (cleaned.StartsWith('(') && cleaned.EndsWith(')'))
            cleaned = "-" + cleaned[1..^1];

        if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            return amount;

        throw new FormatException($"Cannot parse amount: '{value}'");
    }
}

/// <summary>
/// Chase CSV: Transaction Date, Post Date, Description, Category, Type, Amount
/// Negative amounts are debits, positive are credits/payments.
/// </summary>
internal class ChaseFormat : CsvFormat
{
    public override string? SourceName => "Chase";

    public override ParsedTransactionDto? ParseRow(CsvReader csv, List<string> warnings, int rowNumber)
    {
        var dateStr = csv.GetField("Transaction Date");
        var postDateStr = csv.GetField("Post Date");
        var description = csv.GetField("Description") ?? "";
        var amountStr = csv.GetField("Amount");

        var date = ParseDate(dateStr, rowNumber, warnings);
        var amount = ParseAmount(amountStr);

        DateOnly? postDate = null;
        if (!string.IsNullOrWhiteSpace(postDateStr))
            postDate = ParseDate(postDateStr, rowNumber, warnings);

        // Chase: negative = purchase/debit, positive = payment/credit
        var isCredit = amount > 0;
        amount = Math.Abs(amount);

        return new ParsedTransactionDto
        {
            TransactionDate = date,
            PostDate = postDate,
            RawDescription = description.Trim(),
            Amount = amount,
            IsCredit = isCredit
        };
    }
}

/// <summary>
/// Mint CSV: Date, Description, Original Description, Amount, Transaction Type, Category, Account Name, Labels, Notes
/// Transaction Type: "debit" or "credit". Amount is always positive.
/// </summary>
internal class MintFormat : CsvFormat
{
    public override string? SourceName => "Mint";

    public override ParsedTransactionDto? ParseRow(CsvReader csv, List<string> warnings, int rowNumber)
    {
        var dateStr = csv.GetField("Date");
        var description = csv.GetField("Original Description") ?? csv.GetField("Description") ?? "";
        var amountStr = csv.GetField("Amount");
        var transType = csv.GetField("Transaction Type")?.ToLowerInvariant();

        var date = ParseDate(dateStr, rowNumber, warnings);
        var amount = ParseAmount(amountStr);
        var isCredit = transType == "credit";

        return new ParsedTransactionDto
        {
            TransactionDate = date,
            RawDescription = description.Trim(),
            Amount = Math.Abs(amount),
            IsCredit = isCredit
        };
    }
}

/// <summary>
/// Generic CSV: expects at minimum Date, Description, Amount columns.
/// Negative amounts = debits, positive = credits.
/// Also handles: Debit, Credit as separate columns.
/// </summary>
internal class GenericFormat : CsvFormat
{
    public override string? SourceName => "Generic";

    public override ParsedTransactionDto? ParseRow(CsvReader csv, List<string> warnings, int rowNumber)
    {
        // Try to find date column
        var dateStr = csv.GetField("Date")
            ?? csv.GetField("Transaction Date")
            ?? csv.GetField("Trans Date");

        if (string.IsNullOrWhiteSpace(dateStr))
        {
            warnings.Add($"Row {rowNumber}: No date found, skipping.");
            return null;
        }

        var description = csv.GetField("Description")
            ?? csv.GetField("Memo")
            ?? csv.GetField("Payee")
            ?? "";

        // Try single Amount column first
        var amountStr = csv.GetField("Amount");
        decimal amount;
        bool isCredit;

        if (!string.IsNullOrWhiteSpace(amountStr))
        {
            amount = ParseAmount(amountStr);
            isCredit = amount > 0;
            amount = Math.Abs(amount);
        }
        else
        {
            // Try separate Debit/Credit columns
            var debitStr = csv.GetField("Debit");
            var creditStr = csv.GetField("Credit");
            var debit = ParseAmount(debitStr);
            var credit = ParseAmount(creditStr);

            if (credit > 0)
            {
                amount = credit;
                isCredit = true;
            }
            else
            {
                amount = Math.Abs(debit);
                isCredit = false;
            }
        }

        var date = ParseDate(dateStr, rowNumber, warnings);

        // Try for post date
        DateOnly? postDate = null;
        var postDateStr = csv.GetField("Post Date") ?? csv.GetField("Posted Date");
        if (!string.IsNullOrWhiteSpace(postDateStr))
        {
            try { postDate = ParseDate(postDateStr, rowNumber, warnings); }
            catch { /* optional field, ignore parse failure */ }
        }

        return new ParsedTransactionDto
        {
            TransactionDate = date,
            PostDate = postDate,
            RawDescription = description.Trim(),
            Amount = amount,
            IsCredit = isCredit
        };
    }
}