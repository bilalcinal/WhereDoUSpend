using System.Security.Claims;
using FinanceTracker.Application.Budgets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetService _service;
    public BudgetsController(IBudgetService service) => _service = service;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var items = await _service.ListAsync(UserId, year, month, ct);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<BudgetVm>> Create([FromBody] BudgetCreateDto dto, CancellationToken ct)
    {
        var vm = await _service.CreateAsync(UserId, dto, ct);
        return CreatedAtAction(nameof(Get), new { id = vm.Id }, vm);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BudgetVm>> Update(int id, [FromBody] BudgetUpdateDto dto, CancellationToken ct)
    {
        var vm = await _service.UpdateAsync(UserId, id, dto, ct);
        return Ok(vm);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(UserId, id, ct);
        return NoContent();
    }
} 