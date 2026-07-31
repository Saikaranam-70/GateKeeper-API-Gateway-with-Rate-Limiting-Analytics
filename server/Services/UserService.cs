using System;
using System.Threading.Tasks;
using GateKeeper.Services;

public class UserService : IUserService
{
    private const string UserProfileCachePrefix = "user:profile:";
    private static readonly TimeSpan UserProfileCacheTtl = TimeSpan.FromMinutes(30);

    private readonly IUserRepository _repository;
    private readonly ICacheService _cache;
    private readonly IJwtService _jwt;
    private readonly IEmailService _emailService;

    public UserService(
        IUserRepository repository,
        ICacheService cache,
        IJwtService jwt,
        IEmailService emailService)
    {
        _repository = repository;
        _cache = cache;
        _jwt = jwt;
        _emailService = emailService;
    }

    public async Task<UserResponseDTO> GetMeAsync(Guid userId)
    {
        var cacheKey = $"{UserProfileCachePrefix}{userId}";

        var cached = await _cache.GetAsync<UserResponseDTO>(cacheKey);
        if (cached != null)
            return cached;

        var user = await _repository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User Not Found");

        var profile = MapToUserResponse(user);
        await _cache.SetAsync(cacheKey, profile, UserProfileCacheTtl);
        return profile;
    }

    public async Task<LoginResponseDTO> LoginAsync(UserRequestDTO.LoginRequestDTO request)
    {
        var user = await _repository.GetByEmailAsync(request.Email);
        if (user == null) throw new Exception("Invalid email or password.");

        bool valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!valid) throw new Exception("Invalid email or password.");

        if (!user.IsEmailVerified)
        {
            // Auto-send fresh OTP code if account is not verified yet
            string newOtp = Random.Shared.Next(100000, 999999).ToString();
            user.OtpCode = newOtp;
            user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(10);
            await _repository.UpdateAsync(user);
            await _emailService.SendOtpEmailAsync(user.Email, user.Name, newOtp);

            throw new Exception("EMAIL_NOT_VERIFIED");
        }

        string token = _jwt.GenerateToken(user);
        await _cache.SetAsync($"token:{user.Id}", token, TimeSpan.FromHours(1));

        return new LoginResponseDTO
        {
            Token = token,
            User = MapToUserResponse(user)
        };
    }

    public async Task<RegisterResponseDTO> RegisterAsync(UserRequestDTO.RegisterRequestDTO request)
    {
        var existing = await _repository.GetByEmailAsync(request.Email);
        string otpCode = Random.Shared.Next(100000, 999999).ToString();

        if (existing != null)
        {
            if (existing.IsEmailVerified)
            {
                throw new Exception("Email already registered. Please log in.");
            }

            // Unverified user registering again: update credentials and send new OTP
            existing.Name = request.Name;
            existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            existing.OtpCode = otpCode;
            existing.OtpExpiresAt = DateTime.UtcNow.AddMinutes(10);
            await _repository.UpdateAsync(existing);
            await _emailService.SendOtpEmailAsync(existing.Email, existing.Name, otpCode);

            return new RegisterResponseDTO
            {
                Message = "Registration pending email verification. A new OTP code was sent to your email.",
                Email = existing.Email,
                RequiresVerification = true
            };
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "USER",
            IsEmailVerified = false,
            OtpCode = otpCode,
            OtpExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        var id = await _repository.CreateAsync(user);
        user.Id = id;

        await _emailService.SendOtpEmailAsync(user.Email, user.Name, otpCode);

        return new RegisterResponseDTO
        {
            Message = "Account registered successfully. Please enter the OTP code sent to your email.",
            Email = user.Email,
            RequiresVerification = true
        };
    }

    public async Task<LoginResponseDTO> VerifyOtpAsync(UserRequestDTO.VerifyOtpRequestDTO request)
    {
        var user = await _repository.GetByEmailAsync(request.Email);
        if (user == null) throw new Exception("User account not found.");

        if (user.IsEmailVerified)
        {
            string existingToken = _jwt.GenerateToken(user);
            return new LoginResponseDTO { Token = existingToken, User = MapToUserResponse(user) };
        }

        if (string.IsNullOrEmpty(user.OtpCode) || user.OtpCode != request.OtpCode)
        {
            throw new Exception("Invalid OTP code.");
        }

        if (!user.OtpExpiresAt.HasValue || user.OtpExpiresAt.Value < DateTime.UtcNow)
        {
            throw new Exception("OTP code has expired. Please request a new code.");
        }

        user.IsEmailVerified = true;
        user.OtpCode = null;
        user.OtpExpiresAt = null;
        await _repository.UpdateAsync(user);

        string token = _jwt.GenerateToken(user);
        await _cache.SetAsync($"token:{user.Id}", token, TimeSpan.FromHours(1));

        return new LoginResponseDTO
        {
            Token = token,
            User = MapToUserResponse(user)
        };
    }

    public async Task<bool> ResendOtpAsync(UserRequestDTO.ResendOtpRequestDTO request)
    {
        var user = await _repository.GetByEmailAsync(request.Email);
        if (user == null) throw new Exception("User not found.");

        if (user.IsEmailVerified)
        {
            throw new Exception("Account email is already verified.");
        }

        string otpCode = Random.Shared.Next(100000, 999999).ToString();
        user.OtpCode = otpCode;
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(10);
        await _repository.UpdateAsync(user);

        await _emailService.SendOtpEmailAsync(user.Email, user.Name, otpCode);
        return true;
    }

    public async Task<bool> ForgotPasswordAsync(UserRequestDTO.ForgotPasswordRequestDTO request)
    {
        var user = await _repository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            // Return true silently to prevent email enumeration
            return true;
        }

        string resetOtp = Random.Shared.Next(100000, 999999).ToString();
        user.ResetOtpCode = resetOtp;
        user.ResetOtpExpiresAt = DateTime.UtcNow.AddMinutes(10);
        await _repository.UpdateAsync(user);

        await _emailService.SendPasswordResetOtpEmailAsync(user.Email, user.Name, resetOtp);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(UserRequestDTO.ResetPasswordRequestDTO request)
    {
        var user = await _repository.GetByEmailAsync(request.Email);
        if (user == null) throw new Exception("User not found.");

        if (string.IsNullOrEmpty(user.ResetOtpCode) || user.ResetOtpCode != request.OtpCode)
        {
            throw new Exception("Invalid reset OTP code.");
        }

        if (!user.ResetOtpExpiresAt.HasValue || user.ResetOtpExpiresAt.Value < DateTime.UtcNow)
        {
            throw new Exception("Password reset OTP code has expired.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ResetOtpCode = null;
        user.ResetOtpExpiresAt = null;
        await _repository.UpdateAsync(user);

        return true;
    }

    private static UserResponseDTO MapToUserResponse(User user)
    {
        return new UserResponseDTO
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = Enum.TryParse<ROLE>(user.Role, true, out var role) ? role : ROLE.USER
        };
    }
}