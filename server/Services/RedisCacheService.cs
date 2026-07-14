using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace GateKeeper.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        private IDatabase Database => _redis.GetDatabase();

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await Database.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                {
                    _logger.LogInformation($"Cache miss for key: {key}");
                    return default;
                }

                _logger.LogInformation($"Cache hit for key: {key}");
                // Cast RedisValue explicitly to string to resolve ambiguity
                string jsonString = value.ToString();
                return JsonSerializer.Deserialize<T>(jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reading from cache for key: {key}");
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                if (expiration.HasValue)
                    await Database.StringSetAsync(key, json, expiration.Value);
                else
                    await Database.StringSetAsync(key, json);
                _logger.LogInformation($"Successfully cached key: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error writing to cache for key: {key}");
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await Database.KeyDeleteAsync(key);
                _logger.LogInformation($"Successfully removed cache key: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing cache key: {key}");
            }
        }
    }
}
