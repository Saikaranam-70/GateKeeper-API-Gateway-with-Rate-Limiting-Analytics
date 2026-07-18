public class RateLimitRuleResponseDTO
{
    public Guid Id { get; set; }
    public Guid GatewayId { get; set; }
    public string Scope { get; set; } = string.Empty;
    public Guid? ApiKeyId { get; set; }
    public int RequestsPerWindow { get; set; }
    public int WindowSeconds { get; set; }
    public string Algorithm { get; set; } = string.Empty;
    public int BurstAllowance { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}