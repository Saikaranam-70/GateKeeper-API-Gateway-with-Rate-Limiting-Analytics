using GateKeeper.Exceptions;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _repository;
    private readonly IGatewayRepository _gatewayRepository;

    public AnalyticsService(IAnalyticsRepository repository, IGatewayRepository gatewayRepository)
    {
        _repository = repository;
        _gatewayRepository = gatewayRepository;
    }

    public async Task<IEnumerable<TrafficMetricResponseDTO>> GetTrafficSummaryAsync(Guid gatewayId, Guid userId, DateTime? from, DateTime? to, int windowHours)
    {
        await EnsureGatewayAccessAsync(gatewayId, userId);
        return await _repository.GetTrafficSummaryAsync(gatewayId, from, to, windowHours);
    }

    public async Task<IEnumerable<LatencyMetricResponseDTO>> GetLatencyAnalyticsAsync(Guid gatewayId, Guid userId, DateTime? from, DateTime? to, int windowHours)
    {
        await EnsureGatewayAccessAsync(gatewayId, userId);
        return await _repository.GetLatencyAnalyticsAsync(gatewayId, from, to, windowHours);
    }

    public async Task<IEnumerable<ErrorMetricResponseDTO>> GetErrorAnalyticsAsync(Guid gatewayId, Guid userId, DateTime? from, DateTime? to, int windowHours)
    {
        await EnsureGatewayAccessAsync(gatewayId, userId);
        return await _repository.GetErrorAnalyticsAsync(gatewayId, from, to, windowHours);
    }

    public async Task<PaginatedRequestLogsResponseDTO> GetRequestLogsAsync(Guid gatewayId, Guid userId, DateTime? from, DateTime? to, int? statusCode, int page, int limit)
    {
        await EnsureGatewayAccessAsync(gatewayId, userId);
        return await _repository.GetRequestLogsAsync(gatewayId, from, to, statusCode, page, limit);
    }

    private async Task EnsureGatewayAccessAsync(Guid gatewayId, Guid userId)
    {
        var allowed = await _gatewayRepository.ExistsForUserAsync(gatewayId, userId);
        if (!allowed)
        {
            throw new NotFoundException("Gateway not found.");
        }
    }
}
