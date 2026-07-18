public interface IAnalyticsRepository
{
    Task<TrafficSummaryResponseDTO> GetTrafficSummaryAsync(Guid gatewayId, DateTime? from, DateTime? to, int windowHours);
    Task<LatencyAnalyticsResponseDTO> GetLatencyAnalyticsAsync(Guid gatewayId, DateTime? from, DateTime? to, int windowHours);
    Task<ErrorAnalyticsResponseDTO> GetErrorAnalyticsAsync(Guid gatewayId, DateTime? from, DateTime? to, int windowHours);
    Task<PaginatedRequestLogsResponseDTO> GetRequestLogsAsync(Guid gatewayId, DateTime? from, DateTime? to, int? statusCode, int page, int limit);
}
