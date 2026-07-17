using GateKeeper.Exceptions;
using GateKeeper.Models.Entities;
using GateKeeper.Services;
using Microsoft.AspNetCore.Http.HttpResults;

public class GatewayService : IGatewayService
{
    private const string GatewayCachePrefix    = "gateway:";
    private const string UserGatewayListPrefix = "gateways:user:";
    private static readonly TimeSpan GatewayTtl = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan ListTtl    = TimeSpan.FromMinutes(10);

    private readonly IGatewayRepository _repository;  // was wrongly public
    private readonly ICacheService _cache;             // was wrongly public

    public GatewayService(IGatewayRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    // ── CREATE ───────────────────────────────────────────────────────────────
    public async Task<CreateGatewayResponseDTO> CreateGatewayAsync(
        GatewayRequestDTO.CreateGatewayRequestDTO request, Guid userId)
    {
        var gateway = new Gateway
        {
            UserId                = userId,
            Name                  = request.Name,
            Description           = request.Description,
            TargetBaseUrl         = request.TargetBaseUrl,
            Status                = "active",
            DefaultRateLimitPerMin = request.DefaultRateLimitPerMin
        };

        var id = await _repository.CreateAsync(gateway);

        if (request.Routes.Any())
        {
            var routes = request.Routes.Select(r => new RouteConfig
            {
                GatewayId   = id,
                Path        = r.Path,
                Methods     = string.Join(",", r.Methods.Select(m => m.ToUpper())),
                StripPrefix = r.StripPrefix,
                IsActive    = true
            });
            await _repository.CreateRoutesAsync(routes);
        }

        // Invalidate user list cache so new gateway appears immediately
        await _cache.RemoveAsync($"{UserGatewayListPrefix}{userId}");

        return new CreateGatewayResponseDTO
        {
            Id        = id,
            GatewayId = $"gw_{id.ToString("N")[..8]}", // e.g. "gw_d2f67430"
            Status    = "active"
        };
    }

    // ── GET BY ID ────────────────────────────────────────────────────────────
    public async Task<GatewayResponseDTO> GetGatewayAsync(Guid id, Guid userId)
    {
        var cacheKey = $"{GatewayCachePrefix}{id}";

        var cached = await _cache.GetAsync<GatewayResponseDTO>(cacheKey);
        if (cached != null) return cached;

        var gateway = await _repository.GetByIdAsync(id);
        if (gateway == null || gateway.UserId != userId)
            throw new Exception("Gateway not found.");

        var routes   = await _repository.GetRoutesByGatewayIdAsync(id);
        var response = MapToDTO(gateway, routes);

        await _cache.SetAsync(cacheKey, response, GatewayTtl);
        return response;
    }

    // ── GET ALL FOR USER ─────────────────────────────────────────────────────
    public async Task<IEnumerable<GatewayResponseDTO>> GetUserGatewaysAsync(Guid userId)
    {
        var cacheKey = $"{UserGatewayListPrefix}{userId}";

        var cached = await _cache.GetAsync<List<GatewayResponseDTO>>(cacheKey);
        if (cached != null) return cached;

        var gateways = await _repository.GetByUserIdAsync(userId);

        // For list view — routes are NOT loaded (saves N+1 queries)
        var result = gateways
            .Select(g => MapToDTO(g, Enumerable.Empty<RouteConfig>()))
            .ToList();

        await _cache.SetAsync(cacheKey, result, ListTtl);
        return result;
    }

    // ── UPDATE ───────────────────────────────────────────────────────────────
    public async Task<GatewayResponseDTO> UpdateGatewayAsync(
        Guid id, GatewayRequestDTO.UpdateGatewayRequestDTO request, Guid userId)
    {
        var gateway = await _repository.GetByIdAsync(id);
        if (gateway == null || gateway.UserId != userId)
            throw new Exception("Gateway not found.");

        // Patch — only update fields that were actually sent
        if (request.Name              != null) gateway.Name               = request.Name;
        if (request.Description       != null) gateway.Description        = request.Description;
        if (request.TargetBaseUrl     != null) gateway.TargetBaseUrl      = request.TargetBaseUrl;
        if (request.Status            != null) gateway.Status             = request.Status;
        if (request.DefaultRateLimitPerMin != null) gateway.DefaultRateLimitPerMin = request.DefaultRateLimitPerMin;

        await _repository.UpdateAsync(gateway);

        // Invalidate both specific gateway cache AND user list cache
        await _cache.RemoveAsync($"{GatewayCachePrefix}{id}");
        await _cache.RemoveAsync($"{UserGatewayListPrefix}{userId}");

        var routes = await _repository.GetRoutesByGatewayIdAsync(id);
        return MapToDTO(gateway, routes);
    }

    // ── DELETE ───────────────────────────────────────────────────────────────
    public async Task DeleteGatewayAsync(Guid id, Guid userId)
    {
        var exists = await _repository.ExistsForUserAsync(id, userId);
        if (!exists) throw new Exception("Gateway not found.");

        await _repository.DeleteAsync(id);


        await _cache.RemoveAsync($"{GatewayCachePrefix}{id}");
        await _cache.RemoveAsync($"{UserGatewayListPrefix}{userId}");
    }


    private static GatewayResponseDTO MapToDTO(Gateway g, IEnumerable<RouteConfig> routes)
    {
        return new GatewayResponseDTO
        {
            Id                    = g.Id,
            Name                  = g.Name,
            Description           = g.Description,
            TargetBaseUrl         = g.TargetBaseUrl,
            Status                = g.Status,
            DefaultRateLimitPerMin = g.DefaultRateLimitPerMin,
            CreatedAt             = g.CreatedAt,
            UpdatedAt             = g.UpdatedAt,
            Routes                = routes.Select(r => new RouteConfigResponseDTO
            {
                Id          = r.Id,
                Path        = r.Path,
                Methods     = r.Methods.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                StripPrefix = r.StripPrefix,
                IsActive    = r.IsActive
            }).ToList()
        };
    }

    public async Task<RouteConfigResponseDTO> AddRouteAsync(Guid id, GatewayRequestDTO.RouteConfigRequestDTO request, Guid userId)
    {
        var isOwner = await _repository.ExistsForUserAsync(id, userId);
        if (!isOwner)
        {
            throw new NotFoundException("Gateway not found.");
        }
        var route = new RouteConfig
        {
            GatewayId = id,
            Path = request.Path,
            Methods = string.Join(",", request.Methods.Select(m => m.ToUpper())),
            StripPrefix = request.StripPrefix,
            IsActive = true
        };

        var createdRoute = await _repository.AddRouteAsync(route);

        await _cache.RemoveAsync($"{GatewayCachePrefix}{id}");
        await _cache.RemoveAsync($"{UserGatewayListPrefix}{userId}");

        return new RouteConfigResponseDTO
        {
            Id = createdRoute.Id,
            Path = createdRoute.Path,
            Methods = createdRoute.Methods.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            StripPrefix = createdRoute.StripPrefix,
            IsActive = createdRoute.IsActive
        };
    }

    public async Task DeleteRouteAsync(Guid id, Guid routeId, Guid userId)
    {
        var isOwner = await _repository.ExistsForUserAsync(id, userId);
        if(!isOwner) throw new NotFoundException("Gateway not found.");

        var route = await _repository.GetRoutesByIdAsync(routeId);
        if(route == null || route.GatewayId != id)
        {
            throw new NotFoundException("Route not found on this gateway");
        }

        await _repository.DeleteRouteAsync(routeId);

        await _cache.RemoveAsync($"{GatewayCachePrefix}{id}");
        await _cache.RemoveAsync($"{UserGatewayListPrefix}{userId}");
    }

    public async Task<IEnumerable<RouteConfigResponseDTO>> GetGatewayRoutesAsync(Guid id, Guid userId)
    {
        var isOwner = await _repository.ExistsForUserAsync(id, userId);
        if (!isOwner)
        {
            throw new NotFoundException("Gateway not Found");
        }

        var routes = await _repository.GetAllRoutesByGatewayIdAsync(id);
        return routes.Select(r => new RouteConfigResponseDTO
        {
            Id = r.Id,
            Path = r.Path,
            Methods = r.Methods.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            StripPrefix = r.StripPrefix,
            IsActive = r.IsActive
        }).ToList();
    }

    public async Task<RouteConfigResponseDTO> UpdateRouteAsync(Guid id, Guid routeId, GatewayRequestDTO.UpdateRouteRequestDTO request, Guid userId)
    {
        var isOwner = await _repository.ExistsForUserAsync(id, userId);
        if (!isOwner)
        {
            throw new NotFoundException("Gateway not found");
        }

        var route = await _repository.GetRoutesByIdAsync(routeId);
        if(route == null || route.GatewayId != id)
        {
            throw new NotFoundException("Route not found on this gateway");
        }

        route.Path = request.Path;
        route.Methods = string.Join(",", request.Methods.Select(m=>m.ToUpper()));
        route.StripPrefix = request.StripPrefix;
        route.IsActive = request.IsActive;

        await _repository.UpdateRouteAsync(route);

        await _cache.RemoveAsync($"{GatewayCachePrefix}{id}");
        await _cache.RemoveAsync($"{UserGatewayListPrefix}{userId}");

        return new RouteConfigResponseDTO
        {
            Id = route.Id,
            Path = route.Path,
            Methods = route.Methods.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            StripPrefix = route.StripPrefix,
            IsActive = route.IsActive
        };

    }

}