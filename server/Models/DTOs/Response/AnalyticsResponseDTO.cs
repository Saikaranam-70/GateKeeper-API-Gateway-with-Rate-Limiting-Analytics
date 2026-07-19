using System;

public class TrafficMetricResponseDTO
{
    public DateTime Timestamp { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessCount { get; set; }
    public int RateLimitedCount { get; set; }
    public int ErrorCount { get; set; }
}

public class LatencyMetricResponseDTO
{
    public DateTime Timestamp { get; set; }
    public double AvgLatencyMs { get; set; }
    public double P50LatencyMs { get; set; }
    public double P95LatencyMs { get; set; }
    public double P99LatencyMs { get; set; }
}

public class ErrorMetricResponseDTO
{
    public int StatusCode { get; set; }
    public int Count { get; set; }
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
