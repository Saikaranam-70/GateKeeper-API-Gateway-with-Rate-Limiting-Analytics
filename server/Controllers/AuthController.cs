using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _service;

    public AuthController(IUserService service)
    {
        _service = service;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized(new { message = "Invalid token." });

        // user.Id is a Guid (UUID) in DB
        if (!Guid.TryParse(userIdStr, out Guid userId))
            return Unauthorized(new { message = "Invalid token." });

        var result = await _service.GetMeAsync(userId);
        return Ok(result);
    }
}
