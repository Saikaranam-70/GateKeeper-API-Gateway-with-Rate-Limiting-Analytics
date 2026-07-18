using System.Linq;
using GateKeeper.Exceptions;
using GateKeeper.Models.Entities;

public class RateLimitRuleService : IRateLimitRuleService
{
    private readonly IRateLimitRuleRepository _repository;
    private readonly IGatewayRepository _gatewayRepository;

    public RateLimitRuleService(IRateLimitRuleRepository repository, IGatewayRepository gatewayRepository)
    {
        _repository = repository;
        _gatewayRepository = gatewayRepository;
    }

    public async Task<IEnumerable<RateLimitRuleResponseDTO>> ListAsync(Guid userId, int page, int limit, Guid? gatewayId)
    {
        var rules = await _repository.GetByUserIdAsync(userId, gatewayId, page, limit);
        return rules.Select(MapToDto).ToList();
    }

    public async Task<RateLimitRuleResponseDTO> CreateAsync(RateLimitRuleRequestDTO.CreateRateLimitRuleRequestDTO request, Guid userId)
    {
        var gatewayExists = await _gatewayRepository.ExistsForUserAsync(request.GatewayId, userId);
        if (!gatewayExists)
            throw new NotFoundException("Gateway not found.");

        var rule = new RateLimitRule
        {
            GatewayId = request.GatewayId,
            Scope = request.Scope,
            ApiKeyId = request.ApiKeyId,
            RequestsPerWindow = request.RequestsPerWindow,
            WindowSeconds = request.WindowSeconds,
            Algorithm = request.Algorithm,
            BurstAllowance = request.BurstAllowance,
            IsActive = request.IsActive
        };

        var created = await _repository.CreateAsync(rule);
        return MapToDto(created);
    }

    public async Task<RateLimitRuleResponseDTO> UpdateAsync(Guid id, RateLimitRuleRequestDTO.UpdateRateLimitRuleRequestDTO request, Guid userId)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException("Rate limit rule not found.");

        var gatewayExists = await _gatewayRepository.ExistsForUserAsync(existing.GatewayId, userId);
        if (!gatewayExists)
            throw new NotFoundException("Gateway not found.");

        if (request.Scope != null) existing.Scope = request.Scope;
        if (request.ApiKeyId.HasValue) existing.ApiKeyId = request.ApiKeyId;
        if (request.RequestsPerWindow.HasValue) existing.RequestsPerWindow = request.RequestsPerWindow.Value;
        if (request.WindowSeconds.HasValue) existing.WindowSeconds = request.WindowSeconds.Value;
        if (request.Algorithm != null) existing.Algorithm = request.Algorithm;
        if (request.BurstAllowance.HasValue) existing.BurstAllowance = request.BurstAllowance.Value;
        if (request.IsActive.HasValue) existing.IsActive = request.IsActive.Value;

        await _repository.UpdateAsync(existing);
        return MapToDto(existing);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new NotFoundException("Rate limit rule not found.");

        var gatewayExists = await _gatewayRepository.ExistsForUserAsync(existing.GatewayId, userId);
        if (!gatewayExists)
            throw new NotFoundException("Gateway not found.");

        await _repository.DeleteAsync(id);
    }

    private static RateLimitRuleResponseDTO MapToDto(RateLimitRule rule)
    {
        return new RateLimitRuleResponseDTO
        {
            Id = rule.Id,
            GatewayId = rule.GatewayId,
            Scope = rule.Scope,
            ApiKeyId = rule.ApiKeyId,
            RequestsPerWindow = rule.RequestsPerWindow,
            WindowSeconds = rule.WindowSeconds,
            Algorithm = rule.Algorithm,
            BurstAllowance = rule.BurstAllowance,
            IsActive = rule.IsActive,
            CreatedAt = rule.CreatedAt
        };
    }
}