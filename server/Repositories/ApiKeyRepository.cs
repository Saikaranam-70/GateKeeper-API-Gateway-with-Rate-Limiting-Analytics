using Dapper;
using GateKeeper.Database;
using GateKeeper.Models.Entities;

public class ApiKeyRepository : IApiKeyRepository
{
    private readonly DbConnectionFactory _factory;

    public ApiKeyRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }


    public async Task<IEnumerable<ApiKey>> GetByUserIdAsync(Guid userId)
    {
        using var connection = _factory.CreateConnection();
        var sql = @"
            SELECT ak.id, ak.gateway_id, ak.key_hash, ak.key_prefix, ak.label,
                   ak.is_active, ak.expires_at, ak.last_used_at, ak.created_at
            FROM api_keys ak
            INNER JOIN gateways g ON g.id = ak.gateway_id
            WHERE g.user_id = @UserId
              AND ak.is_active = TRUE
            ORDER BY ak.created_at DESC";
        return await connection.QueryAsync<ApiKey>(sql, new { UserId = userId });
    }


    public async Task<ApiKey> CreateAsync(ApiKey apiKey)
    {
        using var connection = _factory.CreateConnection();
        var sql = @"
            INSERT INTO api_keys (gateway_id, key_hash, key_prefix, label, is_active, expires_at)
            VALUES (@GatewayId, @KeyHash, @KeyPrefix, @Label, @IsActive, @ExpiresAt)
            RETURNING id, gateway_id, key_hash, key_prefix, label, is_active, expires_at, last_used_at, created_at";
        return await connection.QuerySingleAsync<ApiKey>(sql, apiKey);
    }


    public async Task<ApiKey?> GetByIdAsync(Guid id)
    {
        using var connection = _factory.CreateConnection();
        var sql = "SELECT * FROM api_keys WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<ApiKey>(sql, new { Id = id });
    }

    
    public async Task<bool> RevokeAsync(Guid id, Guid userId)
    {
        using var connection = _factory.CreateConnection();
        var sql = @"
            UPDATE api_keys ak
            SET is_active = FALSE
            FROM gateways g
            WHERE ak.id = @Id
              AND ak.gateway_id = g.id
              AND g.user_id = @UserId
              AND ak.is_active = TRUE";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, UserId = userId });
        return rows > 0;
    }
}