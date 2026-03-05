using Asp.Versioning;
using BudgetTracker.Application.Common;
using BudgetTracker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class DataController : ControllerBase
{
    private readonly BudgetDbContext _context;
    private readonly IWebHostEnvironment _env;

    public DataController(BudgetDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet("backup")]
    public IActionResult Backup()
    {
        var dbPath = GetDatabasePath();

        if (!System.IO.File.Exists(dbPath))
            return NotFound(ApiResponse<object>.Fail("Database file not found."));

        return PhysicalFile(dbPath, "application/x-sqlite3", "budget-backup.db");
    }

    [HttpPost("restore")]
    public async Task<ActionResult<ApiResponse<bool>>> Restore(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<bool>.Fail("No file uploaded."));

        if (!file.FileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiResponse<bool>.Fail("Only .db files are accepted."));

        var dbPath = GetDatabasePath();

        await _context.Database.CloseConnectionAsync();

        var tempPath = dbPath + ".tmp";
        await using (var stream = new FileStream(tempPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        System.IO.File.Copy(tempPath, dbPath, overwrite: true);
        System.IO.File.Delete(tempPath);

        return Ok(ApiResponse<bool>.Ok(true, "Database restored. Restart the application to apply changes."));
    }

    private string GetDatabasePath()
    {
        var dbPath = Path.Combine(_env.ContentRootPath, "..", "..", "data", "budget.db");
        return Path.GetFullPath(dbPath);
    }
}
