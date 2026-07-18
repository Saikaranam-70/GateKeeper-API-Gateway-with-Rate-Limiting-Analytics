using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/rate-limits")]
[Authorize]
public class RateLimitController : ControllerBase
{
    private readonly IRateLimitRuleService _service;

    public RateLimitController(IRateLimitRuleService service)
    {
        _service = service;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int limit = 50, [FromQuery] Guid? gatewayId = null)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _service.ListAsync(userId, page, limit, gatewayId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RateLimitRuleRequestDTO.CreateRateLimitRuleRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _service.CreateAsync(request, userId);
        return StatusCode(201, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RateLimitRuleRequestDTO.UpdateRateLimitRuleRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _service.UpdateAsync(id, request, userId);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        await _service.DeleteAsync(id, userId);
        return NoContent();
    }
}