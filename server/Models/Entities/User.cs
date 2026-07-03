public class User
{
    public long Id {get; set;}
    public Guid Uid {get; set;} = Guid.NewGuid();
    public string Name {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string PasswordHash {get; set;} = string.Empty;
    public ROLE Role {get; set;} = ROLE.USER;
    public DateTime CreatedAt {get; set;}
    public DateTime UpdatedAt {get; set;}
}