using System.Security.Claims;
using FinanceTracker.Application.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _service;
    public AccountsController(IAccountService service) => _service = service;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var (items, total) = await _service.ListAsync(UserId, page, pageSize, ct);
        return Ok(new { items, total, page, pageSize });
    }

    [HttpPost]
    public async Task<ActionResult<AccountVm>> Create([FromBody] AccountCreateDto dto, CancellationToken ct)
    {
        var vm = await _service.CreateAsync(UserId, dto, ct);
        return CreatedAtAction(nameof(Get), new { id = vm.Id }, vm);
    }

    [HttpPatch("{id:int}/archive")]
    public async Task<IActionResult> Archive(int id, CancellationToken ct)
    {
        await _service.ArchiveAsync(UserId, id, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(UserId, id, ct);
        return NoContent();
    }
} 