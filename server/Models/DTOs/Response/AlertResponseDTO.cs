public class AlertResponseDTO
{
    public Guid Id { get; set; }
    public Guid GatewayId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MetricType { get; set; } = string.Empty;
    public decimal ThresholdValue { get; set; }
    public string ThresholdUnit { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AlertStatsResponseDTO
{
    public int TotalAlerts { get; set; }
    public int ActiveAlerts { get; set; }
    public int InactiveAlerts { get; set; }
    public int TotalGateways { get; set; }
    public int TotalUsers { get; set; }
}
