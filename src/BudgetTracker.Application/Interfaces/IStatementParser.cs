using BudgetTracker.Application.DTOs;

namespace BudgetTracker.Application.Interfaces;

public interface IStatementParser
{
    Task<ParseResultDto> ParseAsync(Stream fileStream, string fileName);
}
