using GateKeeper.Services;

public class UserService : IUserService
{
    private const string UserProfileCachePrefix = "user:profile:";
    private static readonly TimeSpan UserProfileCacheTtl = TimeSpan.FromMinutes(30);

    private readonly IUserRepository _repository;
    private readonly ICacheService _cache;
    private readonly IJwtService _jwt;

    public UserService(IUserRepository repository, ICacheService cache, IJwtService jwt)
    {
        _repository = repository;
        _jwt = jwt;
        _cache = cache;
    }

    public async Task<UserResponseDTO> GetMeAsync(Guid userId)
    {
        var cacheKey = $"{UserProfileCachePrefix}{userId}";

        var cached = await _cache.GetAsync<UserResponseDTO>(cacheKey);
        if (cached != null)
            return cached;

        var user = await _repository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User Not Found");

        var profile = new UserResponseDTO
        {
            Id = user.Id,   // UUID primary key from DB
            Name = user.Name,
            Email = user.Email,
            Role = Enum.TryParse<ROLE>(user.Role, true, out var role) ? role : ROLE.USER
        };

        await _cache.SetAsync(cacheKey, profile, UserProfileCacheTtl);
        return profile;
    }

    public async Task<LoginResponseDTO> LoginAsync(UserRequestDTO.LoginRequestDTO request)
    {
        var user = await _repository.GetByEmailAsync(request.Email);
        if (user == null) throw new Exception("Invalid email or password.");

        bool valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!valid) throw new Exception("Invalid email or password.");

        string token = _jwt.GenerateToken(user);

        // Cache token keyed by the UUID primary key
        await _cache.SetAsync($"token:{user.Id}", token, TimeSpan.FromHours(1));

        return new LoginResponseDTO
        {
            Token = token,
            User = new UserResponseDTO
            {
                Id = user.Id,   // UUID primary key from DB
                Name = user.Name,
                Email = user.Email,
                Role = Enum.TryParse<ROLE>(user.Role, true, out var role) ? role : ROLE.USER
            }
        };
    }

    async Task<UserResponseDTO> IUserService.RegisterAsync(UserRequestDTO.RegisterRequestDTO request)
    {
        var existing = await _repository.GetByEmailAsync(request.Email);
        if (existing != null) throw new Exception("Email already Exists");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "USER"
        };

        var id = await _repository.CreateAsync(user);
        user.Id = id;

        return new UserResponseDTO
        {
            Id = user.Id,   // UUID primary key returned from DB after INSERT
            Name = user.Name,
            Email = user.Email,
            Role = ROLE.USER
        };
    }
}