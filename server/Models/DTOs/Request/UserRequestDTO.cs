public class UserRequestDTO
{
    public class RegisterRequestDTO
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginRequestDTO
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class VerifyOtpRequestDTO
    {
        public string Email { get; set; } = "";
        public string OtpCode { get; set; } = "";
    }

    public class ResendOtpRequestDTO
    {
        public string Email { get; set; } = "";
    }

    public class ForgotPasswordRequestDTO
    {
        public string Email { get; set; } = "";
    }

    public class ResetPasswordRequestDTO
    {
        public string Email { get; set; } = "";
        public string OtpCode { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}