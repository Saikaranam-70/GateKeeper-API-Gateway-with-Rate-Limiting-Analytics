using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _service;

    public AnalyticsController(IAnalyticsService service)
    {
        _service = service;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [HttpGet("{gatewayId:guid}")]
    public async Task<IActionResult> GetTrafficSummary(Guid gatewayId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int windowHours = 24)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _service.GetTrafficSummaryAsync(gatewayId, userId, from, to, windowHours);
        return Ok(result);
    }

    [HttpGet("{gatewayId:guid}/latency")]
    public async Task<IActionResult> GetLatency(Guid gatewayId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int windowHours = 24)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _service.GetLatencyAnalyticsAsync(gatewayId, userId, from, to, windowHours);
        return Ok(result);
    }

    [HttpGet("{gatewayId:guid}/errors")]
    public async Task<IActionResult> GetErrors(Guid gatewayId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int windowHours = 24)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _service.GetErrorAnalyticsAsync(gatewayId, userId, from, to, windowHours);
        return Ok(result);
    }

    [HttpGet("{gatewayId:guid}/requests")]
    public async Task<IActionResult> GetRequests(Guid gatewayId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? statusCode, [FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        if (page < 1) page = 1;
        if (limit < 1) limit = 50;
        if (limit > 200) limit = 200;

        var result = await _service.GetRequestLogsAsync(gatewayId, userId, from, to, statusCode, page, limit);
        return Ok(result);
    }
}
