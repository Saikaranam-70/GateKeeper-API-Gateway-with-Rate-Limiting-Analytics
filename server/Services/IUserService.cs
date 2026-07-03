public interface IUserService
{
    Task<LoginResponseDTO> LoginAsync(UserRequestDTO.LoginRequestDTO request);
    Task<UserResponseDTO> RegisterAsync(UserRequestDTO.RegisterRequestDTO request);
}