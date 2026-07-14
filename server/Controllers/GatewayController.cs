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


    
}
