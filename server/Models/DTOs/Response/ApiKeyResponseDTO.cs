public class ApiKeyResponseDTO
{
    public Guid Id { get; set; }
    public Guid GatewayId { get; set; }
    public string KeyPrefix { get; set; } = string.Empty;
    public string? Label { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Returned ONLY on creation — the full raw key is shown once and never stored plain.
/// </summary>
public class GenerateApiKeyResponseDTO
{
    public Guid Id { get; set; }
    public string RawApiKey { get; set; } = string.Empty;  // ⚠️ shown once only
    public string KeyPrefix { get; set; } = string.Empty;
    public string? Label { get; set; }
    public Guid GatewayId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}