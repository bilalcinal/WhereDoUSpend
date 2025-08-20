using System.Security.Claims;
using FinanceTracker.Application.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _service;
    public TransactionsController(ITransactionService service) => _service = service;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? accountId, [FromQuery] int? categoryId, [FromQuery] string? sort = "date:desc", [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var (items, total) = await _service.ListAsync(UserId, from, to, accountId, categoryId, sort, page, pageSize, ct);
        return Ok(new { items, total, page, pageSize });
    }

    [HttpPost]
    public async Task<ActionResult<TransactionVm>> Create([FromBody] TransactionCreateDto dto, CancellationToken ct)
    {
        var vm = await _service.CreateAsync(UserId, dto, ct);
        return CreatedAtAction(nameof(Get), new { id = vm.Id }, vm);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TransactionVm>> Update(int id, [FromBody] TransactionUpdateDto dto, CancellationToken ct)
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