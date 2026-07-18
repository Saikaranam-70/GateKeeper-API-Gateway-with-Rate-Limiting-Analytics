using System;

namespace GateKeeper.Models.Entities;
public class RateLimitRule
{
    public Guid Id { get; set; }
    public Guid GatewayId { get; set; }
    public string Scope { get; set; } = string.Empty;
    public Guid? ApiKeyId { get; set; }
    public int RequestsPerWindow { get; set; }
    public int WindowSeconds { get; set; }
    public string Algorithm { get; set; } = "sliding-window";
    public int BurstAllowance { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}