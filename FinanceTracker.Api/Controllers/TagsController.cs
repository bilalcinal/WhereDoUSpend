using System.Security.Claims;
using FinanceTracker.Application.Tags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ITagService _service;
    public TagsController(ITagService service) => _service = service;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        var (items, total) = await _service.ListAsync(UserId, page, pageSize, search, ct);
        return Ok(new { items, total, page, pageSize });
    }

    [HttpPost]
    public async Task<ActionResult<TagVm>> Create([FromBody] TagCreateDto dto, CancellationToken ct)
    {
        var vm = await _service.CreateAsync(UserId, dto, ct);
        return CreatedAtAction(nameof(Get), new { id = vm.Id }, vm);
    }
} 