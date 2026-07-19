using Dapper;
using GateKeeper.Database;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly DbConnectionFactory _factory;

    public AnalyticsRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<TrafficMetricResponseDTO>> GetTrafficSummaryAsync(Guid gatewayId, DateTime? from, DateTime? to, int windowHours)
    {
        using var connection = _factory.CreateConnection();

        var sql = @"
            SELECT
                date_trunc('hour', timestamp) AS Timestamp,
                COUNT(*)::int AS TotalRequests,
                SUM(CASE WHEN status_code >= 200 AND status_code < 400 THEN 1 ELSE 0 END)::int AS SuccessCount,
                SUM(CASE WHEN is_rate_limited = TRUE THEN 1 ELSE 0 END)::int AS RateLimitedCount,
                SUM(CASE WHEN status_code >= 400 THEN 1 ELSE 0 END)::int AS ErrorCount
            FROM request_logs
            WHERE gateway_id = @GatewayId
              AND (@From IS NULL OR timestamp >= @From)
              AND (@To IS NULL OR timestamp <= @To)
              AND (@WindowHours IS NULL OR timestamp >= NOW() - (@WindowHours || ' hours')::interval)
            GROUP BY date_trunc('hour', timestamp)
            ORDER BY Timestamp ASC";

        return await connection.QueryAsync<TrafficMetricResponseDTO>(sql, new { GatewayId = gatewayId, From = from, To = to, WindowHours = windowHours });
    }

    public async Task<IEnumerable<LatencyMetricResponseDTO>> GetLatencyAnalyticsAsync(Guid gatewayId, DateTime? from, DateTime? to, int windowHours)
    {
        using var connection = _factory.CreateConnection();

        var sql = @"
            SELECT
                date_trunc('hour', timestamp) AS Timestamp,
                COALESCE(AVG(latency_ms), 0)::float AS AvgLatencyMs,
                COALESCE(percentile_cont(0.50) WITHIN GROUP (ORDER BY latency_ms), 0)::float AS P50LatencyMs,
                COALESCE(percentile_cont(0.95) WITHIN GROUP (ORDER BY latency_ms), 0)::float AS P95LatencyMs,
                COALESCE(percentile_cont(0.99) WITHIN GROUP (ORDER BY latency_ms), 0)::float AS P99LatencyMs
            FROM request_logs
            WHERE gateway_id = @GatewayId
              AND (@From IS NULL OR timestamp >= @From)
              AND (@To IS NULL OR timestamp <= @To)
              AND (@WindowHours IS NULL OR timestamp >= NOW() - (@WindowHours || ' hours')::interval)
            GROUP BY date_trunc('hour', timestamp)
            ORDER BY Timestamp ASC";

        return await connection.QueryAsync<LatencyMetricResponseDTO>(sql, new { GatewayId = gatewayId, From = from, To = to, WindowHours = windowHours });
    }

    public async Task<IEnumerable<ErrorMetricResponseDTO>> GetErrorAnalyticsAsync(Guid gatewayId, DateTime? from, DateTime? to, int windowHours)
    {
        using var connection = _factory.CreateConnection();

        var sql = @"
            SELECT
                status_code AS StatusCode,
                COUNT(*)::int AS Count
            FROM request_logs
            WHERE gateway_id = @GatewayId
              AND status_code >= 400
              AND (@From IS NULL OR timestamp >= @From)
              AND (@To IS NULL OR timestamp <= @To)
              AND (@WindowHours IS NULL OR timestamp >= NOW() - (@WindowHours || ' hours')::interval)
            GROUP BY status_code
            ORDER BY Count DESC";

        return await connection.QueryAsync<ErrorMetricResponseDTO>(sql, new { GatewayId = gatewayId, From = from, To = to, WindowHours = windowHours });
    }

    public async Task<PaginatedRequestLogsResponseDTO> GetRequestLogsAsync(Guid gatewayId, DateTime? from, DateTime? to, int? statusCode, int page, int limit)
    {
        using var connection = _factory.CreateConnection();

        var offset = (page - 1) * limit;

        var countSql = @"
            SELECT COUNT(*) FROM request_logs
            WHERE gateway_id = @GatewayId
              AND (@From IS NULL OR timestamp >= @From)
              AND (@To IS NULL OR timestamp <= @To)
              AND (@StatusCode IS NULL OR status_code = @StatusCode)";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { GatewayId = gatewayId, From = from, To = to, StatusCode = statusCode });

        var sql = @"
            SELECT
                id,
                gateway_id AS GatewayId,
                api_key_id AS ApiKeyId,
                method AS Method,
                path AS Path,
                status_code AS StatusCode,
                latency_ms AS LatencyMs,
                client_ip AS ClientIp,
                is_rate_limited AS IsRateLimited,
                timestamp AS Timestamp
            FROM request_logs
            WHERE gateway_id = @GatewayId
              AND (@From IS NULL OR timestamp >= @From)
              AND (@To IS NULL OR timestamp <= @To)
              AND (@StatusCode IS NULL OR status_code = @StatusCode)
            ORDER BY timestamp DESC
            LIMIT @Limit OFFSET @Offset";

        var items = (await connection.QueryAsync<RequestLogItemResponseDTO>(sql, new { GatewayId = gatewayId, From = from, To = to, StatusCode = statusCode, Limit = limit, Offset = offset })).ToList();

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / limit);

        return new PaginatedRequestLogsResponseDTO
        {
            Page = page,
            PageSize = limit,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Items = items
        };
    }
}
