using Dapper;
using GateKeeper.Database;
using GateKeeper.Models.Entities;

public class GatewayRepository : IGatewayRepository
{
    private readonly DbConnectionFactory _factory;

    public GatewayRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<Guid> CreateAsync(Gateway gateway)
    {
        using var connection = _factory.CreateConnection();
        var sql = @"
            INSERT INTO gateways (user_id, name, description, target_base_url, status, default_rate_limit_per_min)
            VALUES (@UserId, @Name, @Description, @TargetBaseUrl, @Status, @DefaultRateLimitPerMin)
            RETURNING id";
        return await connection.QuerySingleAsync<Guid>(sql, gateway);
    }

    public async Task CreateRoutesAsync(IEnumerable<RouteConfig> routes)
    {
        using var connection = _factory.CreateConnection();
        var sql = @"
            INSERT INTO route_configs (gateway_id, path, methods, strip_prefix, is_active)
            VALUES (@GatewayId, @Path, @Methods, @StripPrefix, @IsActive)";
        await connection.ExecuteAsync(sql, routes);
    }

    public async Task<Gateway?> GetByIdAsync(Guid id)
    {
        using var connection = _factory.CreateConnection();
        var sql = "SELECT * FROM gateways WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Gateway>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Gateway>> GetByUserIdAsync(Guid userId)
    {
        using var connection = _factory.CreateConnection();
        var sql = "SELECT * FROM gateways WHERE user_id = @UserId ORDER BY created_at DESC";
        return await connection.QueryAsync<Gateway>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<RouteConfig>> GetRoutesByGatewayIdAsync(Guid gatewayId)
    {
        using var connection = _factory.CreateConnection();
        var sql = "SELECT * FROM route_configs WHERE gateway_id = @GatewayId AND is_active = TRUE";
        return await connection.QueryAsync<RouteConfig>(sql, new { GatewayId = gatewayId });
    }

    public async Task UpdateAsync(Gateway gateway)
    {
        using var connection = _factory.CreateConnection();
        var sql = @"
            UPDATE gateways
            SET name                      = @Name,
                description               = @Description,
                target_base_url           = @TargetBaseUrl,
                status                    = @Status,
                default_rate_limit_per_min = @DefaultRateLimitPerMin,
                updated_at                = NOW()
            WHERE id = @Id";
        await connection.ExecuteAsync(sql, gateway);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM gateways WHERE id = @Id", new { Id = id });
    }

    public async Task<bool> ExistsForUserAsync(Guid id, Guid userId)
    {
        using var connection = _factory.CreateConnection();
        var sql = "SELECT COUNT(1) FROM gateways WHERE id = @Id AND user_id = @UserId";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id, UserId = userId });
        return count > 0;
    }
}