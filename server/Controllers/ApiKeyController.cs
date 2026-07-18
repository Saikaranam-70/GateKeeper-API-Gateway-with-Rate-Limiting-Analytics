using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/api-keys")]
[Authorize]
public class ApiKeyController: ControllerBase
{
    private readonly IApiKeyService _service;
    public ApiKeyController(IApiKeyService service)
    {
        _service = service;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(claim, out var id) ? id: Guid.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int limit = 50, [FromQuery] Guid? gatewayId = null)
    {
        var userId = GetUserId();
        if(userId == Guid.Empty) return Unauthorized();

        var keys = await _service.ListApiKeysAsync(userId, page, limit, gatewayId);
        return Ok(keys);
    }

    [HttpPost]
    public async Task<IActionResult> Generate([FromBody] ApiKeyRequestDTO.GenerateApiKeyRequestDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        if(userId == Guid.Empty) return Unauthorized();

        var result = await _service.GenerateApiKeyAsync(request, userId);
        return StatusCode(201, result);
    }

    [HttpDelete("{keyId:guid}")]
    public async Task<IActionResult> Revoke(Guid keyId)
    {
        var userId = GetUserId();
        if(userId == Guid.Empty) return Unauthorized();

        await _service.RevokeApiKeyAsync(keyId, userId);
        return NoContent();
    }

}