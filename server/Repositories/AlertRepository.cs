using Dapper;
using GateKeeper.Database;
using GateKeeper.Models.Entities;

public class AlertRepository : IAlertRepository
{
    private readonly DbConnectionFactory _factory;

    public AlertRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Alert>> GetByUserIdAsync(Guid userId, Guid? gatewayId, int page, int limit)
    {
        using var connection = _factory.CreateConnection();
        var offset = (page - 1) * limit;

        var sql = @"
            SELECT a.*
            FROM alerts a
            INNER JOIN gateways g ON g.id = a.gateway_id
            WHERE g.user_id = @UserId
              AND (@GatewayId IS NULL OR a.gateway_id = @GatewayId)
            ORDER BY a.created_at DESC
            LIMIT @Limit OFFSET @Offset";

        return await connection.QueryAsync<Alert>(sql, new { UserId = userId, GatewayId = gatewayId, Limit = limit, Offset = offset });
    }

    public async Task<Alert> CreateAsync(Alert alert)
    {
        using var connection = _factory.CreateConnection();

        var sql = @"
            INSERT INTO alerts (gateway_id, name, metric_type, threshold_value, threshold_unit, is_active)
            VALUES (@GatewayId, @Name, @MetricType, @ThresholdValue, @ThresholdUnit, @IsActive)
            RETURNING id, gateway_id, name, metric_type, threshold_value, threshold_unit, is_active, last_triggered_at, created_at";

        return await connection.QuerySingleAsync<Alert>(sql, alert);
    }

    public async Task<Alert?> GetByIdAsync(Guid id)
    {
        using var connection = _factory.CreateConnection();
        var sql = "SELECT * FROM alerts WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Alert>(sql, new { Id = id });
    }

    public async Task<Alert> UpdateAsync(Alert alert)
    {
        using var connection = _factory.CreateConnection();

        var sql = @"
            UPDATE alerts
            SET name = @Name,
                metric_type = @MetricType,
                threshold_value = @ThresholdValue,
                threshold_unit = @ThresholdUnit,
                is_active = @IsActive
            WHERE id = @Id
            RETURNING id, gateway_id, name, metric_type, threshold_value, threshold_unit, is_active, last_triggered_at, created_at";

        return await connection.QuerySingleAsync<Alert>(sql, alert);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM alerts WHERE id = @Id", new { Id = id });
    }

    public async Task<AlertStatsResponseDTO> GetPlatformStatsAsync()
    {
        using var connection = _factory.CreateConnection();

        var sql = @"
            SELECT
                (SELECT COUNT(*) FROM alerts) AS TotalAlerts,
                (SELECT COUNT(*) FROM alerts WHERE is_active = TRUE) AS ActiveAlerts,
                (SELECT COUNT(*) FROM alerts WHERE is_active = FALSE) AS InactiveAlerts,
                (SELECT COUNT(*) FROM gateways) AS TotalGateways,
                (SELECT COUNT(*) FROM users) AS TotalUsers";

        return await connection.QuerySingleAsync<AlertStatsResponseDTO>(sql);
    }
}
