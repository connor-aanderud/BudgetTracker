using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using UglyToad.PdfPig;

namespace BudgetTracker.Application.Services.Parsing;

/// <summary>
/// Parses FNBO (First National Bank of Omaha) SCHEELS Visa PDF statements.
/// Uses PdfPig to extract words, reconstructs visual lines (group by vertical
/// position, order left-to-right), then hands the lines to
/// <see cref="FnboStatementTextParser"/> for the actual transaction parsing.
/// </summary>
public class FnboStatementParser : IStatementParser
{
    public bool CanParse(string fileName) =>
        fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

    public async Task<ParseResultDto> ParseAsync(Stream fileStream, string fileName)
    {
        using var buffer = new MemoryStream();
        await fileStream.CopyToAsync(buffer);

        var lines = ExtractLines(buffer.ToArray());
        return FnboStatementTextParser.Parse(lines, fileName);
    }

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
