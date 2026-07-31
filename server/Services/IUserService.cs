public interface IUserService
{
    Task<UserResponseDTO> GetMeAsync(Guid userId);
    Task<LoginResponseDTO> LoginAsync(UserRequestDTO.LoginRequestDTO request);
    Task<RegisterResponseDTO> RegisterAsync(UserRequestDTO.RegisterRequestDTO request);
    Task<LoginResponseDTO> VerifyOtpAsync(UserRequestDTO.VerifyOtpRequestDTO request);
    Task<bool> ResendOtpAsync(UserRequestDTO.ResendOtpRequestDTO request);
    Task<bool> ForgotPasswordAsync(UserRequestDTO.ForgotPasswordRequestDTO request);
    Task<bool> ResetPasswordAsync(UserRequestDTO.ResetPasswordRequestDTO request);
}

public class RegisterResponseDTO
{
    public string Message { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool RequiresVerification { get; set; } = true;
}