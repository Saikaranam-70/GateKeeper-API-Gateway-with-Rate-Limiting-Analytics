public interface IApiKeyService
{
    Task<GenerateApiKeyResponseDTO> GenerateApiKeyAsync(ApiKeyRequestDTO.GenerateApiKeyRequestDTO request, Guid userId);
    Task<IEnumerable<ApiKeyResponseDTO>> ListApiKeysAsync(Guid userId, int page, int limit, Guid? gatewayId);
    Task RevokeApiKeyAsync(Guid keyId, Guid userId);
}