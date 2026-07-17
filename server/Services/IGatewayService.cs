public interface IGatewayService
{
    Task<RouteConfigResponseDTO> AddRouteAsync(Guid id, GatewayRequestDTO.RouteConfigRequestDTO request, Guid userId);
    Task<CreateGatewayResponseDTO> CreateGatewayAsync(GatewayRequestDTO.CreateGatewayRequestDTO request, Guid userId);
    Task DeleteGatewayAsync(Guid id, Guid userId);
    Task DeleteRouteAsync(Guid id, Guid routeId, Guid userId);
    Task<GatewayResponseDTO> GetGatewayAsync(Guid id, Guid userId);
    Task<IEnumerable<RouteConfigResponseDTO>> GetGatewayRoutesAsync(Guid id, Guid userId);
    Task<IEnumerable<GatewayResponseDTO>> GetUserGatewaysAsync(Guid userId);
    Task<GatewayResponseDTO> UpdateGatewayAsync(Guid id, GatewayRequestDTO.UpdateGatewayRequestDTO request, Guid userId);
    Task<RouteConfigResponseDTO> UpdateRouteAsync(Guid id, Guid routeId, GatewayRequestDTO.UpdateRouteRequestDTO request, Guid userId);
}