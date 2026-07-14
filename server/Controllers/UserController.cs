using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRequestDTO.RegisterRequestDTO request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserRequestDTO.LoginRequestDTO request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.LoginAsync(request);
        return Ok(result);
    }
}