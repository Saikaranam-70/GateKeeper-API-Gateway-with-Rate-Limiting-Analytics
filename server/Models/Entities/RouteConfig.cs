public class RouteConfig
{
    public Guid Id {get; set;}
    public Guid GatewayId {get; set;}
    public string Path {get; set;} = string.Empty;
    public string Methods {get; set;} = string.Empty;
    public bool StripPrefix {get; set;} = false;
    public bool IsActive {get; set;}
    public DateTime CreatedAt {get; set;}
}