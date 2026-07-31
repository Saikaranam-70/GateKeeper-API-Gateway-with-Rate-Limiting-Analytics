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
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserRequestDTO.LoginRequestDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _service.LoginAsync(request);
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message == "EMAIL_NOT_VERIFIED")
        {
            return BadRequest(new { message = "Email is not verified. A verification OTP code has been sent to your email.", requiresVerification = true, email = request.Email });
        }
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(UserRequestDTO.VerifyOtpRequestDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _service.VerifyOtpAsync(request);
        return Ok(result);
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(UserRequestDTO.ResendOtpRequestDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _service.ResendOtpAsync(request);
        return Ok(new { message = "A new verification OTP code has been sent to your email." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(UserRequestDTO.ForgotPasswordRequestDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _service.ForgotPasswordAsync(request);
        return Ok(new { message = "If the email is registered, a password reset OTP code has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(UserRequestDTO.ResetPasswordRequestDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _service.ResetPasswordAsync(request);
        return Ok(new { message = "Password has been reset successfully. You can now log in with your new password." });
    }
}