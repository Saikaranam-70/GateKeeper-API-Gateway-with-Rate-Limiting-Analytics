public class GatewayResponseDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TargetBaseUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? DefaultRateLimitPerMin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<RouteConfigResponseDTO> Routes { get; set; } = new();
}

public class RouteConfigResponseDTO
{
    public Guid Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public List<string> Methods { get; set; } = new();
    public bool StripPrefix { get; set; }
    public bool IsActive { get; set; }
}

public class CreateGatewayResponseDTO
{
    public Guid Id { get; set; }
    public string GatewayId { get; set; } = string.Empty; // "gw_XXXXXXXX" short display id
    public string Status { get; set; } = string.Empty;
}