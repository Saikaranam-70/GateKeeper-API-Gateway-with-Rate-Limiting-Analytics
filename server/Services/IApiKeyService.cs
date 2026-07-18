public interface IApiKeyService
{
    Task<GenerateApiKeyResponseDTO> GenerateApiKeyAsync(ApiKeyRequestDTO.GenerateApiKeyRequestDTO request, Guid userId);
    Task<IEnumerable<ApiKeyResponseDTO>> ListApiKeysAsync(Guid userId);
    Task RevokeApiKeyAsync(Guid keyId, Guid userId);
}