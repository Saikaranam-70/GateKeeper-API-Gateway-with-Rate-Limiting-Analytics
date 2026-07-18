using System.ComponentModel.DataAnnotations;

public class ApiKeyRequestDTO
{
    public class GenerateApiKeyRequestDTO
    {
        [Required]
        public Guid GatewayId {get; set;}
        [MaxLength(100)]
        public string? Label {get; set;}
        public DateTime? ExpiresAt{get; set;}
    }
}