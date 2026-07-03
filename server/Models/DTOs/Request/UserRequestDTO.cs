public class UserRequestDTO
{
    public class RegisterRequestDTO
    {
        public string Name {get; set;} = "";
        public string Email {get; set;} = "";
        public string Password {get; set;} = "";
    }

    public class LoginRequestDTO
    {
        public string Email {get; set;} = "";
        public string Password {get; set;} ="";
    }
}