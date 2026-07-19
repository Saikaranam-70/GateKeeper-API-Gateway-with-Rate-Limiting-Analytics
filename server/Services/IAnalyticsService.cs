public interface IAnalyticsService
{
    Task<IEnumerable<TrafficMetricResponseDTO>> GetTrafficSummaryAsync(Guid gatewayId, Guid userId, DateTime? from, DateTime? to, int windowHours);
    Task<IEnumerable<LatencyMetricResponseDTO>> GetLatencyAnalyticsAsync(Guid gatewayId, Guid userId, DateTime? from, DateTime? to, int windowHours);
    Task<IEnumerable<ErrorMetricResponseDTO>> GetErrorAnalyticsAsync(Guid gatewayId, Guid userId, DateTime? from, DateTime? to, int windowHours);
    Task<PaginatedRequestLogsResponseDTO> GetRequestLogsAsync(Guid gatewayId, Guid userId, DateTime? from, DateTime? to, int? statusCode, int page, int limit);
}
