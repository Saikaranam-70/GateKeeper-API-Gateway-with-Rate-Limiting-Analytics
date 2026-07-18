public interface IAnalyticsService
{
    Task<TrafficSummaryResponseDTO> GetTrafficSummaryAsync(Guid gatewayId, Guid userId, DateTime? from, DateTime? to, int windowHours);
    Task<LatencyAnalyticsResponseDTO> GetLatencyAnalyticsAsync(Guid gatewayId, Guid userId, DateTime? from, DateTime? to, int windowHours);
    Task<ErrorAnalyticsResponseDTO> GetErrorAnalyticsAsync(Guid gatewayId, Guid userId, DateTime? from, DateTime? to, int windowHours);
    Task<PaginatedRequestLogsResponseDTO> GetRequestLogsAsync(Guid gatewayId, Guid userId, DateTime? from, DateTime? to, int? statusCode, int page, int limit);
}
