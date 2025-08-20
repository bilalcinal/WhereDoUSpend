using System.Globalization;
using System.Security.Claims;
using System.Text;
using FinanceTracker.Application.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ImportExportController : ControllerBase
{
    private readonly ITransactionService _transactions;
    public ImportExportController(ITransactionService transactions) => _transactions = transactions;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpPost("import/transactions")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Import([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0) return BadRequest("file required");
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        // CSV columns: Date,Amount,Type(1|2),Note,AccountId?,CategoryId?
        string? line;
        var count = 0;
        await reader.ReadLineAsync(); // skip header
        while ((line = await reader.ReadLineAsync()) != null)
        {
            var parts = line.Split(',');
            if (parts.Length < 4) continue;
            var date = DateTime.Parse(parts[0], CultureInfo.InvariantCulture);
            var amount = decimal.Parse(parts[1], CultureInfo.InvariantCulture);
            var type = (FinanceTracker.Domain.TransactionType)int.Parse(parts[2]);
            var note = parts[3];
            int? accountId = parts.Length > 4 && int.TryParse(parts[4], out var aid) ? aid : null;
            int? categoryId = parts.Length > 5 && int.TryParse(parts[5], out var cid) ? cid : null;
            var dto = new TransactionCreateDto(amount, type, date, note, accountId, categoryId, null);
            await _transactions.CreateAsync(UserId, dto, ct);
            count++;
        }
        return Ok(new { imported = count });
    }

    [HttpGet("export/transactions.csv")]
    public async Task<IActionResult> Export([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
    {
        var (items, _) = await _transactions.ListAsync(UserId, from, to, null, null, "date:desc", 1, int.MaxValue, ct);
        var sb = new StringBuilder();
        sb.AppendLine("Date,Amount,Type,Note,Account,Category,Tags");
        foreach (var t in items)
        {
            sb.AppendLine($"{t.Date:yyyy-MM-dd},{t.Amount},{(int)t.Type},\"{t.Note}\",\"{t.AccountName}\",\"{t.CategoryName}\",\"{string.Join('|', t.Tags)}\"");
        }
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "transactions.csv");
    }
} 