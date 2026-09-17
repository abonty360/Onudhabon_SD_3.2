namespace Onudhabon.Services
{
    public interface IEmailService
    {
        Task<bool> SendVerificationEmailAsync(string toEmail, string recipientName, string verificationUrl);
        Task<bool> SendOtpEmailAsync(string toEmail, string recipientName, string otp, string verificationUrl);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string recipientName, string resetUrl);
        Task<bool> SendPasswordResetOtpEmailAsync(string toEmail, string recipientName, string otp);
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}
