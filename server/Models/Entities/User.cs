public class User
{
    public Guid Id { get; set; }          // UUID PRIMARY KEY in DB
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "USER";  // VARCHAR in DB
    public Guid Uid { get; set; }          // Added in V8 migration
    public bool IsEmailVerified { get; set; } = false;
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiresAt { get; set; }
    public string? ResetOtpCode { get; set; }
    public DateTime? ResetOtpExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}