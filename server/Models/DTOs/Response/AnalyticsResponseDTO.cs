public class TrafficSummaryResponseDTO
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int ErrorRequests { get; set; }
    public int RateLimitedRequests { get; set; }
    public double AverageLatencyMs { get; set; }
    public double SuccessRate { get; set; }
    public int WindowHours { get; set; } = 24;
}

public class LatencyPointResponseDTO
{
    public string Route { get; set; } = string.Empty;
    public double P50Ms { get; set; }
    public double P95Ms { get; set; }
    public double P99Ms { get; set; }
    public int RequestCount { get; set; }
}

public class LatencyAnalyticsResponseDTO
{
    public List<LatencyPointResponseDTO> Points { get; set; } = new();
    public int WindowHours { get; set; } = 24;
}

public class ErrorPointResponseDTO
{
    public DateTime Bucket { get; set; }
    public int FourXXCount { get; set; }
    public int FiveXXCount { get; set; }
    public int TotalRequests { get; set; }
}

public class ErrorAnalyticsResponseDTO
{
    public List<ErrorPointResponseDTO> Points { get; set; } = new();
    public int WindowHours { get; set; } = 24;
}

public class RequestLogItemResponseDTO
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

public class PaginatedRequestLogsResponseDTO
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public List<RequestLogItemResponseDTO> Items { get; set; } = new();
}
