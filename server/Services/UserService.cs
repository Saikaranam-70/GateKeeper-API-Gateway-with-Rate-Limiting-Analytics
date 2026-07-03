using GateKeeper.Services;

public class UserService : IUserService
{

    private readonly IUserRepository _repository;
    private readonly ICacheService _cache;
    private readonly IJwtService _jwt;
    public UserService(IUserRepository repository, ICacheService cache, IJwtService jwt)
    {
        _repository = repository;
        _jwt = jwt;
        _cache = cache;
    }

    public async Task<LoginResponseDTO> LoginAsync(UserRequestDTO.LoginRequestDTO request)
    {
        var user = await _repository.GetByEmailAsync(request.Email);
        if(user==null) throw new Exception("Invalid email or password.");

        bool valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if(!valid) throw new Exception("Invalid email or password.");

        string token = _jwt.GenerateToken(user);

        await _cache.SetAsync($"token:{user.Id}", token, TimeSpan.FromHours(1));

        return new LoginResponseDTO
        {
            Token = token,
            User = new UserResponseDTO
            {
                Id = user.Uid,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            }
        };
    }

    async Task<UserResponseDTO> IUserService.RegisterAsync(UserRequestDTO.RegisterRequestDTO request)
    {
        var existing = await _repository.GetByEmailAsync(request.Email);
        if(existing != null) throw new Exception("Email already Exists");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var id = await _repository.CreateAsync(user);
        user.Id = id;
        return new UserResponseDTO
        {
            Id = user.Uid,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }
}