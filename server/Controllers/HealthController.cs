using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StackExchange.Redis;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IConnectionMultiplexer _redis;

    public HealthController(IConfiguration configuration, IConnectionMultiplexer redis)
    {
        _configuration = configuration;
        _redis = redis;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var status = new
        {
            status = "ok",
            checks = new
            {
                sql = await CheckSqlAsync(),
                redis = await CheckRedisAsync(),
                yarp = new { status = "ok" }
            }
        };

        return Ok(status);
    }

    private async Task<object> CheckSqlAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("Postgres"));
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync();
            return new { status = "ok" };
        }
        catch (Exception ex)
        {
            return new { status = "error", error = ex.Message };
        }
    }

    private async Task<object> CheckRedisAsync()
    {
        try
        {
            var ping = await _redis.GetDatabase().PingAsync();
            return new { status = "ok", pingMs = ping.TotalMilliseconds };
        }
        catch (Exception ex)
        {
            return new { status = "error", error = ex.Message };
        }
    }
}
