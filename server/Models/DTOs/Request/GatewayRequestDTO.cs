using System.ComponentModel.DataAnnotations;

public class GatewayRequestDTO
{
    public class CreateGatewayRequestDTO
    {
        [Required]
        [MaxLength(200)]
        public string Name {get; set;} = string.Empty;

        [MaxLength(200)]
        public string? Description {get; set;}

        [Required]
        [MaxLength(500)]
        public string TargetBaseUrl {get; set;} = string.Empty;

        [Range(1, 10000)]
        public int? DefaultRateLimitPerMin {get; set;} = 100;

        public List<RouteConfigRequestDTO> Routes {get; set;} = new();
    }

    public class UpdateGatewayRequestDTO
    {
        [MaxLength(200)]
        public string? Name {get; set;} = string.Empty;

        [MaxLength(500)]
        public string? Description {get; set;}
        [MaxLength(500)]
        public string? TargetBaseUrl {get; set;}
        public string? Status { get; set; }
        [Range(1, 10000)]
        public int? DefaultRateLimitPerMin {get; set;}
    }
    public class RouteConfigRequestDTO
    {
        [Required]
        [MaxLength(500)]
        public string Path { get; set; } = string.Empty;
        [Required]
        public List<string> Methods { get; set; } = new(); // ["GET","POST"]
        public bool StripPrefix { get; set; } = false;
    }
}