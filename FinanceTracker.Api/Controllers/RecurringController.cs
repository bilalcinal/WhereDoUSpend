using System.Security.Claims;
using FinanceTracker.Application.Recurring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecurringController : ControllerBase
{
    private readonly IRecurringService _service;
    public RecurringController(IRecurringService service) => _service = service;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var items = await _service.ListAsync(UserId, ct);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<RecurringVm>> Create([FromBody] RecurringCreateDto dto, CancellationToken ct)
    {
        var vm = await _service.CreateAsync(UserId, dto, ct);
        return CreatedAtAction(nameof(Get), new { id = vm.Id }, vm);
    }

    [HttpPost("run-due")]
    public async Task<IActionResult> RunDue(CancellationToken ct)
    {
        var count = await _service.RunDueAsync(UserId, null, ct);
        return Ok(new { created = count });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(UserId, id, ct);
        return NoContent();
    }
} 