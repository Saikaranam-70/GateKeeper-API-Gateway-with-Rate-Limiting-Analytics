using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GatewayController: ControllerBase
{
    private readonly IGatewayService _service;

    public GatewayController(IGatewayService service)
    {
        _service = service;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GatewayRequestDTO.CreateGatewayRequestDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var userId = GetUserId();
        if(userId == Guid.Empty) return Unauthorized();

        var result = await _service.CreateGatewayAsync(request, userId);
        return StatusCode(201, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if(userId == Guid.Empty) return Unauthorized();

        var result = await _service.GetUserGatewaysAsync(userId);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        if(userId == Guid.Empty) return Unauthorized();

        var result = await _service.GetGatewayAsync(id, userId);
        return Ok(result);

    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] GatewayRequestDTO.UpdateGatewayRequestDTO request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);
        var userId = GetUserId();
        if(userId == Guid.Empty) return Unauthorized();
        var result = await _service.UpdateGatewayAsync(id, request, userId);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        if(userId == Guid.Empty) return Unauthorized();

        await _service.DeleteGatewayAsync(id, userId);
        return NoContent();
    }

    [HttpGet("{id:guid}/routes")]
    public async Task<IActionResult> GetRoutes(Guid id)
    {
        var userId = GetUserId();
        if(userId == Guid.Empty) return Unauthorized();

        var routes = await _service.GetGatewayRoutesAsync(id, userId);
        return Ok(routes);
    }

    [HttpPost("{id:guid}/routes")]
    public async Task<IActionResult> AddRoute(Guid id, [FromBody] GatewayRequestDTO.RouteConfigRequestDTO request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        if(userId == Guid.Empty) return Unauthorized();

        var createdRoute = await _service.AddRouteAsync(id, request, userId);
        return StatusCode(201, createdRoute);
    }

    [HttpPut("{id:guid}/routes/{routeId:guid}")]
    public async Task<IActionResult> UpdateRoute(Guid id, Guid routeId, [FromBody] GatewayRequestDTO.UpdateRouteRequestDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        if(userId == Guid.Empty) return Unauthorized();

        var updateRoute = await _service.UpdateRouteAsync(id, routeId, request, userId);
        return Ok(updateRoute);
    }

    [HttpDelete("{id:guid}/routes/{routeId:guid}")]
    public async Task<IActionResult> DeleteRoute(Guid id, Guid routeId)
    {
        var userId = GetUserId();
        if(userId == Guid.Empty) return Unauthorized();

        await _service.DeleteRouteAsync(id, routeId, userId);
        return NoContent();
    }

    [HttpPost("{id:guid}/simulate-traffic")]
    public async Task<IActionResult> SimulateTraffic(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _service.SimulateTrafficAsync(id, userId);
        return Ok(result);
    }

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] GatewayRequestDTO.UpdateGatewayStatusRequestDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _service.UpdateGatewayStatusAsync(id, request.Status, userId);
        return Ok(result);
    }
}
