using System.ComponentModel.DataAnnotations;

public class RateLimitRuleRequestDTO
{
    public class CreateRateLimitRuleRequestDTO
    {
        [Required]
        public Guid GatewayId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Scope { get; set; } = string.Empty;

        public Guid? ApiKeyId { get; set; }

        [Range(1, 100000)]
        public int RequestsPerWindow { get; set; }

        [Range(1, 86400)]
        public int WindowSeconds { get; set; }

        [MaxLength(30)]
        public string Algorithm { get; set; } = "sliding-window";

        [Range(0, 100000)]
        public int BurstAllowance { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateRateLimitRuleRequestDTO
    {
        [MaxLength(20)]
        public string? Scope { get; set; }

        public Guid? ApiKeyId { get; set; }

        [Range(1, 100000)]
        public int? RequestsPerWindow { get; set; }

        [Range(1, 86400)]
        public int? WindowSeconds { get; set; }

        [MaxLength(30)]
        public string? Algorithm { get; set; }

        [Range(0, 100000)]
        public int? BurstAllowance { get; set; }

        public bool? IsActive { get; set; }
    }
}