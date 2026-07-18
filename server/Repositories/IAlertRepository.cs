using GateKeeper.Models.Entities;

public interface IAlertRepository
{
    Task<IEnumerable<Alert>> GetByUserIdAsync(Guid userId, Guid? gatewayId, int page, int limit);
    Task<Alert> CreateAsync(Alert alert);
    Task<Alert?> GetByIdAsync(Guid id);
    Task<Alert> UpdateAsync(Alert alert);
    Task DeleteAsync(Guid id);
    Task<AlertStatsResponseDTO> GetPlatformStatsAsync();
}
