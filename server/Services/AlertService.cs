using Dapper;
using GateKeeper.Exceptions;
using GateKeeper.Models.Entities;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _repository;
    private readonly IGatewayRepository _gatewayRepository;
    private readonly IUserRepository _userRepository;

    public AlertService(IAlertRepository repository, IGatewayRepository gatewayRepository, IUserRepository userRepository)
    {
        _repository = repository;
        _gatewayRepository = gatewayRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<AlertResponseDTO>> ListAsync(Guid userId, int page, int limit, Guid? gatewayId)
    {
        var alerts = await _repository.GetByUserIdAsync(userId, gatewayId, page, limit);
        return alerts.Select(MapToDto).ToList();
    }

    public async Task<AlertResponseDTO> CreateAsync(AlertRequestDTO.CreateAlertRequestDTO request, Guid userId)
    {
        var gatewayExists = await _gatewayRepository.ExistsForUserAsync(request.GatewayId, userId);
        if (!gatewayExists)
            throw new NotFoundException("Gateway not found.");

        var alert = new Alert
        {
            GatewayId = request.GatewayId,
            Name = request.Name,
            MetricType = request.MetricType,
            ThresholdValue = request.ThresholdValue,
            ThresholdUnit = request.ThresholdUnit,
            IsActive = request.IsActive
        };

        var created = await _repository.CreateAsync(alert);
        return MapToDto(created);
    }

    public async Task<AlertResponseDTO> UpdateAsync(Guid id, AlertRequestDTO.UpdateAlertRequestDTO request, Guid userId)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException("Alert not found.");

        var gatewayExists = await _gatewayRepository.ExistsForUserAsync(existing.GatewayId, userId);
        if (!gatewayExists)
            throw new NotFoundException("Gateway not found.");

        if (request.Name != null) existing.Name = request.Name;
        if (request.MetricType != null) existing.MetricType = request.MetricType;
        if (request.ThresholdValue.HasValue) existing.ThresholdValue = request.ThresholdValue.Value;
        if (request.ThresholdUnit != null) existing.ThresholdUnit = request.ThresholdUnit;
        if (request.IsActive.HasValue) existing.IsActive = request.IsActive.Value;

        var updated = await _repository.UpdateAsync(existing);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException("Alert not found.");

        var gatewayExists = await _gatewayRepository.ExistsForUserAsync(existing.GatewayId, userId);
        if (!gatewayExists)
            throw new NotFoundException("Gateway not found.");

        await _repository.DeleteAsync(id);
    }

    public async Task<AlertStatsResponseDTO> GetPlatformStatsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Role?.ToUpperInvariant() != "ADMIN")
            throw new UnauthorizedAccessException("Admin access required.");

        return await _repository.GetPlatformStatsAsync();
    }

    private static AlertResponseDTO MapToDto(Alert alert)
    {
        return new AlertResponseDTO
        {
            Id = alert.Id,
            GatewayId = alert.GatewayId,
            Name = alert.Name,
            MetricType = alert.MetricType,
            ThresholdValue = alert.ThresholdValue,
            ThresholdUnit = alert.ThresholdUnit,
            IsActive = alert.IsActive,
            LastTriggeredAt = alert.LastTriggeredAt,
            CreatedAt = alert.CreatedAt
        };
    }
}
