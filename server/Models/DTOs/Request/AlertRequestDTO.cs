using System.ComponentModel.DataAnnotations;

public class AlertRequestDTO
{
    public class CreateAlertRequestDTO
    {
        [Required]
        public Guid GatewayId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string MetricType { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public decimal ThresholdValue { get; set; }

        [Required]
        [MaxLength(20)]
        public string ThresholdUnit { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateAlertRequestDTO
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? MetricType { get; set; }

        [Range(0, 1000000)]
        public decimal? ThresholdValue { get; set; }

        [MaxLength(20)]
        public string? ThresholdUnit { get; set; }

        public bool? IsActive { get; set; }
    }
}
