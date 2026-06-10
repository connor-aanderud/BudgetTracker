using BudgetTracker.Application.DTOs;

namespace BudgetTracker.Application.Interfaces;

public interface IStatementParser
{
    /// <summary>
    /// Returns true if this parser knows how to handle the given file (by extension/format).
    /// </summary>
    bool CanParse(string fileName);

    Task<ParseResultDto> ParseAsync(Stream fileStream, string fileName);
}
