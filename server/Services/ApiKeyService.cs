using System.Security.Cryptography;
using GateKeeper.Exceptions;
using GateKeeper.Models.Entities;
using GateKeeper.Services;

public class ApiKeyService : IApiKeyService
{

    private const string UserApiKeyListPrefix = "apikeys:user:";
    private static readonly TimeSpan ListTtl = TimeSpan.FromMinutes(10);

    private readonly IApiKeyRepository _repository;
    private readonly IGatewayRepository _gatewayRepository;
    private readonly ICacheService _cache;

    public ApiKeyService(IApiKeyRepository repository, IGatewayRepository gatewayRepository, ICacheService cache)
    {
        _repository = repository;
        _gatewayRepository = gatewayRepository;
        _cache = cache;
    }

    // ── GENERATE ─────────────────────────────────────────────────────────────
    public async Task<GenerateApiKeyResponseDTO> GenerateApiKeyAsync(ApiKeyRequestDTO.GenerateApiKeyRequestDTO request, Guid userId)
    {
        var isOwner = await _gatewayRepository.ExistsForUserAsync(request.GatewayId, userId);

        if(!isOwner) throw new NotFoundException("Gateway not found");

        // Cryptographically secure random key — format: gk_<48 hex chars>
        var rawBytes  = RandomNumberGenerator.GetBytes(24);
        var rawHex    = Convert.ToHexString(rawBytes).ToLower();
        var rawKey    = $"gk_{rawHex}";
        var keyPrefix = rawKey[..11]; // e.g. "gk_a1b2c3d4"

        // SHA-256 hash — never store the plain key
        var keyHashBytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawKey));
        var keyHash      = Convert.ToHexString(keyHashBytes).ToLower();

        var entity = new ApiKey
        {
            GatewayId = request.GatewayId,
            KeyHash   = keyHash,
            KeyPrefix = keyPrefix,
            Label     = request.Label,
            IsActive  = true,
            ExpiresAt = request.ExpiresAt
        };

        var created = await _repository.CreateAsync(entity);

        // Invalidate list cache — new key must appear immediately
        await _cache.RemoveAsync($"{UserApiKeyListPrefix}{userId}");

        return new GenerateApiKeyResponseDTO
        {
            Id        = created.Id,
            RawApiKey = rawKey,          // ⚠️ returned once — never stored plain
            KeyPrefix = created.KeyPrefix,
            Label     = created.Label,
            GatewayId = created.GatewayId,
            ExpiresAt = created.ExpiresAt,
            CreatedAt = created.CreatedAt
        };
    }

    // ── LIST ─────────────────────────────────────────────────────────────────
    public async Task<IEnumerable<ApiKeyResponseDTO>> ListApiKeysAsync(Guid userId, int page, int limit, Guid? gatewayId)
    {
        var cacheKey = $"{UserApiKeyListPrefix}{userId}:{gatewayId ?? Guid.Empty}:{page}:{limit}";

        var cached = await _cache.GetAsync<List<ApiKeyResponseDTO>>(cacheKey);
        if(cached != null) return cached;

        var keys   = await _repository.GetByUserIdAsync(userId, gatewayId, page, limit);
        var result = keys.Select(MapToDTO).ToList();

        await _cache.SetAsync(cacheKey, result, ListTtl);
        return result;
    }

    // ── REVOKE ────────────────────────────────────────────────────────────────
    public async Task RevokeApiKeyAsync(Guid keyId, Guid userId)
    {
        var revoked = await _repository.RevokeAsync(keyId, userId);
        if (!revoked)
            throw new NotFoundException("API key not found or already revoked.");

        // Invalidate list cache — revoked key must disappear immediately
        await _cache.RemoveAsync($"{UserApiKeyListPrefix}{userId}");
    }

    // ── MAPPER ───────────────────────────────────────────────────────────────
    private static ApiKeyResponseDTO MapToDTO(ApiKey k) =>
        new ApiKeyResponseDTO
        {
            Id         = k.Id,
            GatewayId  = k.GatewayId,
            KeyPrefix  = k.KeyPrefix,
            Label      = k.Label,
            IsActive   = k.IsActive,
            ExpiresAt  = k.ExpiresAt,
            LastUsedAt = k.LastUsedAt,
            CreatedAt  = k.CreatedAt
        };

}
