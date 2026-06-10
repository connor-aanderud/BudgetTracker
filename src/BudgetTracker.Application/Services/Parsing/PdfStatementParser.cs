using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using UglyToad.PdfPig;

namespace BudgetTracker.Application.Services.Parsing;

/// <summary>
/// Parses PDF credit-card statements. Uses PdfPig to extract words, reconstructs
/// visual lines (group by vertical position, order left-to-right), detects which
/// bank's statement it is from the text, and delegates to the matching bank parser.
/// Mirrors how <see cref="CsvStatementParser"/> detects CSV formats internally.
/// </summary>
public class PdfStatementParser : IStatementParser
{
    private enum BankFormat { Unknown, Fnbo, Chase }

    public bool CanParse(string fileName) =>
        fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

    public async Task<ParseResultDto> ParseAsync(Stream fileStream, string fileName)
    {
        using var buffer = new MemoryStream();
        await fileStream.CopyToAsync(buffer);

        var lines = ExtractLines(buffer.ToArray());

        return DetectBank(lines) switch
        {
            BankFormat.Fnbo => FnboStatementTextParser.Parse(lines, fileName),
            BankFormat.Chase => ChaseStatementTextParser.Parse(lines, fileName),
            _ => Unrecognized(fileName)
        };
    }

    private static BankFormat DetectBank(IReadOnlyList<string> lines)
    {
        foreach (var line in lines)
        {
            if (line.Contains("FNBO", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("First National Bank of Omaha", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("card.fnbo.com", StringComparison.OrdinalIgnoreCase))
                return BankFormat.Fnbo;

            if (line.Contains("chase.com", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("Prime Visa", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("CARDMEMBER SERVICE", StringComparison.OrdinalIgnoreCase))
                return BankFormat.Chase;
        }

        return BankFormat.Unknown;
    }

    private static ParseResultDto Unrecognized(string fileName) => new()
    {
        FileName = fileName,
        Warnings = new List<string>
        {
            "Unrecognized PDF statement format — could not identify the bank (expected FNBO or Chase)."
        }
    };

    /// <summary>
    /// Reconstructs text lines from a PDF: words are grouped by their baseline
    /// (rounded vertical position) and ordered left-to-right within each line,
    /// top of the page first.
    /// </summary>
    private static List<string> ExtractLines(byte[] pdfBytes)
    {
        var lines = new List<string>();

        using var document = PdfDocument.Open(pdfBytes);
        foreach (var page in document.GetPages())
        {
            var byLine = page.GetWords()
                .GroupBy(w => Math.Round(w.BoundingBox.Bottom))
                .OrderByDescending(g => g.Key);

            foreach (var lineWords in byLine)
            {
                var text = string.Join(" ", lineWords
                    .OrderBy(w => w.BoundingBox.Left)
                    .Select(w => w.Text));
                lines.Add(text);
            }
        }

        return lines;
    }
}
