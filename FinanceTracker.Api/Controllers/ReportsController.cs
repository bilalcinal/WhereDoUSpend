using System.Security.Claims;
using FinanceTracker.Application.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;
    public ReportsController(IReportService service) => _service = service;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var data = await _service.SummaryAsync(UserId, year, month, ct);
        return Ok(data);
    }

    [HttpGet("cashflow")]
    public async Task<IActionResult> Cashflow([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string groupBy = "day", CancellationToken ct = default)
    {
        var data = await _service.CashflowAsync(UserId, from, to, groupBy, ct);
        return Ok(data);
    }

    [HttpGet("by-account")]
    public async Task<IActionResult> ByAccount([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
    {
        var data = await _service.ByAccountAsync(UserId, from, to, ct);
        return Ok(data);
    }

    [HttpGet("budget-vs-actual")]
    public async Task<IActionResult> BudgetVsActual([FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var data = await _service.BudgetVsActualAsync(UserId, year, month, ct);
        return Ok(data);
    }
} 