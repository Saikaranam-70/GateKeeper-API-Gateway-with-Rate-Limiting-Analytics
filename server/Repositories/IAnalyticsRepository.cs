public interface IAnalyticsRepository
{
    Task<IEnumerable<TrafficMetricResponseDTO>> GetTrafficSummaryAsync(Guid gatewayId, DateTime? from, DateTime? to, int windowHours);
    Task<IEnumerable<LatencyMetricResponseDTO>> GetLatencyAnalyticsAsync(Guid gatewayId, DateTime? from, DateTime? to, int windowHours);
    Task<IEnumerable<ErrorMetricResponseDTO>> GetErrorAnalyticsAsync(Guid gatewayId, DateTime? from, DateTime? to, int windowHours);
    Task<PaginatedRequestLogsResponseDTO> GetRequestLogsAsync(Guid gatewayId, DateTime? from, DateTime? to, int? statusCode, int page, int limit);
}
