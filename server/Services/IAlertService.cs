public interface IAlertService
{
    Task<IEnumerable<AlertResponseDTO>> ListAsync(Guid userId, int page, int limit, Guid? gatewayId);
    Task<AlertResponseDTO> CreateAsync(AlertRequestDTO.CreateAlertRequestDTO request, Guid userId);
    Task<AlertResponseDTO> UpdateAsync(Guid id, AlertRequestDTO.UpdateAlertRequestDTO request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<AlertStatsResponseDTO> GetPlatformStatsAsync(Guid userId);
}
