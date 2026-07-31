namespace GateKeeper.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string name, string otpCode);
        Task SendPasswordResetOtpEmailAsync(string toEmail, string name, string otpCode);
    }
}
