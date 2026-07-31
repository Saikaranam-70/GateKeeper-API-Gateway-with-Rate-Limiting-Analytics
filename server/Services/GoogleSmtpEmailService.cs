using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GateKeeper.Services
{
    public class GoogleSmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleSmtpEmailService> _logger;

        public GoogleSmtpEmailService(IConfiguration configuration, ILogger<GoogleSmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOtpEmailAsync(string toEmail, string name, string otpCode)
        {
            string subject = "GateKeeper - Account Verification Code";
            string htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #6366f1; text-align: center;'>Welcome to GateKeeper!</h2>
                    <p>Hi <strong>{WebUtility.HtmlEncode(name)}</strong>,</p>
                    <p>Thank you for registering. Please use the following 6-digit OTP code to verify your email address:</p>
                    <div style='background-color: #f3f4f6; padding: 15px; text-align: center; border-radius: 6px; font-size: 28px; font-weight: bold; letter-spacing: 5px; color: #4f46e5; margin: 20px 0;'>
                        {otpCode}
                    </div>
                    <p style='color: #6b7280; font-size: 14px;'>This code is valid for 10 minutes. If you did not create an account, please ignore this email.</p>
                </div>";

            await SendEmailAsync(toEmail, subject, htmlBody, otpCode, "Account Verification");
        }

        public async Task SendPasswordResetOtpEmailAsync(string toEmail, string name, string otpCode)
        {
            string subject = "GateKeeper - Password Reset Code";
            string htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #6366f1; text-align: center;'>Password Reset Request</h2>
                    <p>Hi <strong>{WebUtility.HtmlEncode(name)}</strong>,</p>
                    <p>We received a request to reset your GateKeeper account password. Use the following 6-digit OTP code to reset your password:</p>
                    <div style='background-color: #f3f4f6; padding: 15px; text-align: center; border-radius: 6px; font-size: 28px; font-weight: bold; letter-spacing: 5px; color: #dc2626; margin: 20px 0;'>
                        {otpCode}
                    </div>
                    <p style='color: #6b7280; font-size: 14px;'>This code is valid for 10 minutes. If you did not request a password reset, please secure your account.</p>
                </div>";

            await SendEmailAsync(toEmail, subject, htmlBody, otpCode, "Password Reset");
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string otpCode, string purpose)
        {
            // Log OTP to console for debugging / local testing visibility
            _logger.LogInformation("==========================================");
            _logger.LogInformation("[EMAIL OTP] Purpose: {Purpose} | To: {Email} | OTP Code: {OtpCode}", purpose, toEmail, otpCode);
            _logger.LogInformation("==========================================");

            var host = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            var portStr = _configuration["Smtp:Port"] ?? "587";
            int.TryParse(portStr, out int port);
            if (port <= 0) port = 587;

            var enableSslStr = _configuration["Smtp:EnableSsl"] ?? "true";
            bool.TryParse(enableSslStr, out bool enableSsl);

            var smtpUser = _configuration["Smtp:User"];
            var smtpPassword = _configuration["Smtp:Password"];
            var senderName = _configuration["Smtp:SenderName"] ?? "GateKeeper";

            // If credentials aren't configured yet, log warning and return
            if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPassword) || smtpUser.Contains("YOUR_"))
            {
                _logger.LogWarning("SMTP credentials not fully set in appsettings.json. OTP code was logged above for local testing.");
                return;
            }

            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(smtpUser, senderName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(host, port);
                smtpClient.EnableSsl = enableSsl;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPassword);

                await smtpClient.SendMailAsync(message);
                _logger.LogInformation("Successfully sent email to {Email} via Google SMTP.", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email} via Google SMTP.", toEmail);
                // We do not rethrow so user flow continues even if SMTP settings have local delivery issue
            }
        }
    }
}
