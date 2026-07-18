using GateKeeper.Models.Entities;

public interface IRateLimitRuleRepository
{
    Task<RateLimitRule> CreateAsync(RateLimitRule rule);
    Task<IEnumerable<RateLimitRule>> GetByUserIdAsync(Guid userId, Guid? gatewayId, int page, int limit);
    Task<RateLimitRule?> GetByIdAsync(Guid id);
    Task UpdateAsync(RateLimitRule rule);
    Task DeleteAsync(Guid id);
}