namespace GateKeeper.Models.Entities;

public class Gateway
{

    public Guid Id { get; set; }

    public Guid UserId { get; set; }


    public string Name { get; set; } = string.Empty;


    public string? Description { get; set; }


    public string TargetBaseUrl { get; set; } = string.Empty;


    public string Status { get; set; } = "active";


    public int? DefaultRateLimitPerMin { get; set; } = 100;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}