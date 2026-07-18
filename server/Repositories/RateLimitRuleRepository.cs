using Dapper;
using GateKeeper.Database;
using GateKeeper.Models.Entities;

public class RateLimitRuleRepository : IRateLimitRuleRepository
{
    private readonly DbConnectionFactory _factory;

    public RateLimitRuleRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<RateLimitRule> CreateAsync(RateLimitRule rule)
    {
        using var connection = _factory.CreateConnection();

        var sql = @"
            INSERT INTO rate_limit_rules
            (gateway_id, scope, api_key_id, requests_per_window, window_seconds, algorithm, burst_allowance, is_active)
            VALUES
            (@GatewayId, @Scope, @ApiKeyId, @RequestsPerWindow, @WindowSeconds, @Algorithm, @BurstAllowance, @IsActive)
            RETURNING id, gateway_id, scope, api_key_id, requests_per_window, window_seconds, algorithm, burst_allowance, is_active, created_at";

        return await connection.QuerySingleAsync<RateLimitRule>(sql, rule);
    }

    public async Task<IEnumerable<RateLimitRule>> GetByUserIdAsync(Guid userId, Guid? gatewayId, int page, int limit)
    {
        using var connection = _factory.CreateConnection();
        var offset = (page - 1) * limit;

        var sql = @"
            SELECT
                r.id,
                r.gateway_id,
                r.scope,
                r.api_key_id,
                r.requests_per_window,
                r.window_seconds,
                r.algorithm,
                r.burst_allowance,
                r.is_active,
                r.created_at
            FROM rate_limit_rules r
            INNER JOIN gateways g ON g.id = r.gateway_id
            WHERE g.user_id = @UserId
              AND (@GatewayId IS NULL OR r.gateway_id = @GatewayId)
            ORDER BY r.created_at DESC
            LIMIT @Limit OFFSET @Offset";

        return await connection.QueryAsync<RateLimitRule>(sql, new { UserId = userId, GatewayId = gatewayId, Limit = limit, Offset = offset });
    }

    public async Task<RateLimitRule?> GetByIdAsync(Guid id)
    {
        using var connection = _factory.CreateConnection();
        var sql = "SELECT * FROM rate_limit_rules WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<RateLimitRule>(sql, new { Id = id });
    }

    public async Task UpdateAsync(RateLimitRule rule)
    {
        using var connection = _factory.CreateConnection();

        var sql = @"
            UPDATE rate_limit_rules
            SET scope = @Scope,
                api_key_id = @ApiKeyId,
                requests_per_window = @RequestsPerWindow,
                window_seconds = @WindowSeconds,
                algorithm = @Algorithm,
                burst_allowance = @BurstAllowance,
                is_active = @IsActive
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, rule);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM rate_limit_rules WHERE id = @Id", new { Id = id });
    }
}