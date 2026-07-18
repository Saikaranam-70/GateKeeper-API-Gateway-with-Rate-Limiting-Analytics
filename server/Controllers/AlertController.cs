using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertController : ControllerBase
{
    private readonly IAlertService _service;

    public AlertController(IAlertService service)
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
    public async Task<IActionResult> Create([FromBody] AlertRequestDTO.CreateAlertRequestDTO request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _service.CreateAsync(request, userId);
        return StatusCode(201, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AlertRequestDTO.UpdateAlertRequestDTO request)
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
