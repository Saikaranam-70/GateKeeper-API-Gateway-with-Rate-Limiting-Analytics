namespace GateKeeper.Models.Entities;

public class RequestLog
{
    public Guid Id { get; set; }
    public Guid GatewayId { get; set; }
    public Guid? ApiKeyId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int LatencyMs { get; set; }
    public string ClientIp { get; set; } = string.Empty;
    public bool IsRateLimited { get; set; }
    public DateTime Timestamp { get; set; }
}
