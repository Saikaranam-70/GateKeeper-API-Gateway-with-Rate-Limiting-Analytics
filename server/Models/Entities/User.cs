public class User
{
    public Guid Id { get; set; }          // UUID PRIMARY KEY in DB
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "USER";  // VARCHAR in DB
    public Guid Uid { get; set; }          // Added in V8 migration
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}