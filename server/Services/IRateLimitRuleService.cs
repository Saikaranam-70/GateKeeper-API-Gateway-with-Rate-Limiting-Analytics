public interface IRateLimitRuleService
{
    Task<IEnumerable<RateLimitRuleResponseDTO>> ListAsync(Guid userId, int page, int limit, Guid? gatewayId);
    Task<RateLimitRuleResponseDTO> CreateAsync(RateLimitRuleRequestDTO.CreateRateLimitRuleRequestDTO request, Guid userId);
    Task<RateLimitRuleResponseDTO> UpdateAsync(Guid id, RateLimitRuleRequestDTO.UpdateRateLimitRuleRequestDTO request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}