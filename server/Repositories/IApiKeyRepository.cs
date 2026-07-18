using GateKeeper.Models.Entities;

public interface IApiKeyRepository
{
    Task<ApiKey> CreateAsync(ApiKey apiKey);
    Task<IEnumerable<ApiKey>> GetByUserIdAsync(Guid userId, Guid? gatewayId, int page, int limit);
    Task<bool> RevokeAsync(Guid keyId, Guid userId);
    Task<ApiKey?> GetByIdAsync(Guid id);
}