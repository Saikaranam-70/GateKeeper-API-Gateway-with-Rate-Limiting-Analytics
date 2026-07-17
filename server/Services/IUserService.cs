public interface IUserService
{
    Task<UserResponseDTO> GetMeAsync(Guid userId);
    Task<LoginResponseDTO> LoginAsync(UserRequestDTO.LoginRequestDTO request);
    Task<UserResponseDTO> RegisterAsync(UserRequestDTO.RegisterRequestDTO request);
}