using GateKeeper.Models.Entities;

public interface IGatewayRepository
{
    Task<Guid> CreateAsync(Gateway gateway);
    Task CreateRoutesAsync(IEnumerable<RouteConfig> routes);
    Task<Gateway?> GetByIdAsync(Guid id);
    Task<IEnumerable<Gateway>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<RouteConfig>> GetRoutesByGatewayIdAsync(Guid gatewayId);
    Task UpdateAsync(Gateway gateway);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsForUserAsync(Guid id, Guid userId);
    Task<RouteConfig?> AddRouteAsync(RouteConfig route);
    Task<RouteConfig?> GetRoutesByIdAsync(Guid routeId);
    Task DeleteRouteAsync(Guid routeId);
    Task<IEnumerable<RouteConfig>> GetAllRoutesByGatewayIdAsync(Guid id);
    Task UpdateRouteAsync(RouteConfig route);
}
