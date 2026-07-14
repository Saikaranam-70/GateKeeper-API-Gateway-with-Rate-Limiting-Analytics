public interface IGatewayService
{
    Task<CreateGatewayResponseDTO> CreateGatewayAsync(GatewayRequestDTO.CreateGatewayRequestDTO request, Guid userId);
    Task DeleteGatewayAsync(Guid id, Guid userId);
    Task<GatewayResponseDTO> GetGatewayAsync(Guid id, Guid userId);
    Task<IEnumerable<GatewayResponseDTO>> GetUserGatewaysAsync(Guid userId);
    Task<GatewayResponseDTO> UpdateGatewayAsync(Guid id, GatewayRequestDTO.UpdateGatewayRequestDTO request, Guid userId);
}