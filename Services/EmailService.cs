using System.Net;
using System.Net.Mail;

namespace Onudhabon.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendVerificationEmailAsync(string toEmail, string recipientName, string verificationUrl)
        {
            var subject = "Verify Your Email Address - Onudhabon";
            var displayName = string.IsNullOrWhiteSpace(recipientName) ? "Learner/Volunteer" : recipientName.Trim();

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>Email Verification</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f8fafc; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='background-color: #f8fafc; padding: 40px 15px;'>
        <tr>
            <td align='center'>
                <table role='presentation' width='100%' style='max-width: 580px; background-color: #ffffff; border-radius: 16px; box-shadow: 0 4px 12px rgba(0,0,0,0.06); overflow: hidden; border: 1px solid #e2e8f0;'>
                    <!-- Brand Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #1e3a8a 0%, #1d4ed8 100%); padding: 32px 30px; text-align: center;'>
                            <h1 style='margin: 0; color: #ffffff; font-size: 26px; font-weight: 800; letter-spacing: -0.5px;'>Onudhabon</h1>
                            <p style='margin: 6px 0 0 0; color: #93c5fd; font-size: 14px;'>Exploring Education & Community Learning</p>
                        </td>
                    </tr>
                    
                    <!-- Content Body -->
                    <tr>
                        <td style='padding: 36px 32px; color: #334155; font-size: 15px; line-height: 1.6;'>
                            <h2 style='margin: 0 0 16px 0; color: #0f172a; font-size: 20px; font-weight: 700;'>Welcome, {displayName}!</h2>
                            
                            <p style='margin: 0 0 18px 0;'>Thank you for registering on Onudhabon. To ensure the security of your account and initiate the administrator review process, please verify your email address by clicking the button below:</p>
                            
                            <!-- Call to Action Button -->
                            <table role='presentation' cellspacing='0' cellpadding='0' style='margin: 28px auto;'>
                                <tr>
                                    <td align='center' style='border-radius: 50px; background: linear-gradient(135deg, #1e3a8a 0%, #1d4ed8 100%);'>
                                        <a href='{verificationUrl}' target='_blank' style='display: inline-block; padding: 14px 34px; color: #ffffff; font-size: 15px; font-weight: 600; text-decoration: none; border-radius: 50px;'>
                                            Verify Email Address
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <p style='margin: 24px 0 8px 0; font-size: 13px; color: #64748b;'>If the button above does not work, copy and paste this link into your web browser:</p>
                            <p style='margin: 0 0 20px 0; font-size: 12px; color: #2563eb; word-break: break-all;'>
                                <a href='{verificationUrl}' style='color: #2563eb; text-decoration: underline;'>{verificationUrl}</a>
                            </p>

                            <div style='background-color: #f1f5f9; border-left: 4px solid #3b82f6; border-radius: 4px; padding: 12px 16px; margin: 24px 0 12px 0;'>
                                <p style='margin: 0; font-size: 13px; color: #475569;'>
                                    <strong>Important:</strong> This verification link will expire in <strong>24 hours</strong>. Once your email is verified, your registration will be reviewed by an administrator for final account activation.
                                </p>
                            </div>

                            <p style='margin: 20px 0 0 0; font-size: 12px; color: #94a3b8;'>If you did not create an account on Onudhabon, you can safely disregard this email.</p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8fafc; padding: 20px 32px; border-top: 1px solid #e2e8f0; text-align: center; color: #94a3b8; font-size: 12px;'>
                            &copy; {DateTime.UtcNow.Year} Onudhabon. All rights reserved.<br />
                            Dedicated to accessible education and volunteer empowerment.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string recipientName, string otp, string verificationUrl)
        {
            var subject = $"Your Onudhabon Verification OTP: {otp}";
            var displayName = string.IsNullOrWhiteSpace(recipientName) ? "Learner/Volunteer" : recipientName.Trim();

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>Your Verification OTP</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f8fafc; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='background-color: #f8fafc; padding: 40px 15px;'>
        <tr>
            <td align='center'>
                <table role='presentation' width='100%' style='max-width: 560px; background-color: #ffffff; border-radius: 16px; box-shadow: 0 4px 14px rgba(0,0,0,0.06); overflow: hidden; border: 1px solid #e2e8f0;'>
                    <!-- Brand Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #1e3a8a 0%, #1d4ed8 100%); padding: 32px 30px; text-align: center;'>
                            <h1 style='margin: 0; color: #ffffff; font-size: 26px; font-weight: 800; letter-spacing: -0.5px;'>Onudhabon</h1>
                            <p style='margin: 6px 0 0 0; color: #bfdbfe; font-size: 14px;'>Exploring Education & Community Learning</p>
                        </td>
                    </tr>
                    
                    <!-- Content Body -->
                    <tr>
                        <td style='padding: 36px 32px; color: #334155; font-size: 15px; line-height: 1.6;'>
                            <h2 style='margin: 0 0 14px 0; color: #0f172a; font-size: 20px; font-weight: 700;'>Hello, {displayName}!</h2>
                            
                            <p style='margin: 0 0 20px 0;'>Thank you for registering on Onudhabon. Use the following One-Time Password (OTP) to verify your email address:</p>
                            
                            <!-- OTP Box -->
                            <div style='margin: 28px 0; text-align: center;'>
                                <div style='display: inline-block; font-size: 36px; letter-spacing: 10px; font-weight: 800; color: #1e3a8a; background: #eff6ff; padding: 18px 36px; border-radius: 14px; border: 2px dashed #3b82f6; font-family: ""Courier New"", Courier, monospace;'>
                                    {otp}
                                </div>
                                <div style='font-size: 12px; color: #64748b; margin-top: 8px;'>This code is valid for <strong>15 minutes</strong>. Do not share it with anyone.</div>
                            </div>

                            <p style='margin: 24px 0 16px 0; font-size: 14px; color: #475569; text-align: center;'>
                                Or verify automatically with a single click:
                            </p>
                            
                            <!-- Call to Action Button -->
                            <table role='presentation' cellspacing='0' cellpadding='0' style='margin: 0 auto 24px auto;'>
                                <tr>
                                    <td align='center' style='border-radius: 50px; background: linear-gradient(135deg, #1e3a8a 0%, #1d4ed8 100%);'>
                                        <a href='{verificationUrl}' target='_blank' style='display: inline-block; padding: 12px 30px; color: #ffffff; font-size: 14px; font-weight: 600; text-decoration: none; border-radius: 50px;'>
                                            Verify Email Automatically
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <div style='background-color: #f1f5f9; border-left: 4px solid #3b82f6; border-radius: 4px; padding: 12px 16px; margin: 20px 0;'>
                                <p style='margin: 0; font-size: 13px; color: #475569;'>
                                    <strong>Next step after OTP verification:</strong> Your account will be reviewed by an administrator for platform access.
                                </p>
                            </div>

                            <p style='margin: 16px 0 0 0; font-size: 12px; color: #94a3b8;'>If you did not initiate this request, you can safely ignore this email.</p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8fafc; padding: 20px 32px; border-top: 1px solid #e2e8f0; text-align: center; color: #94a3b8; font-size: 12px;'>
                            &copy; {DateTime.UtcNow.Year} Onudhabon. All rights reserved.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string recipientName, string resetUrl)
        {
            var subject = "Reset Your Password - Onudhabon";
            var displayName = string.IsNullOrWhiteSpace(recipientName) ? "User" : recipientName.Trim();

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>Reset Your Password</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f8fafc; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='background-color: #f8fafc; padding: 40px 15px;'>
        <tr>
            <td align='center'>
                <table role='presentation' width='100%' style='max-width: 580px; background-color: #ffffff; border-radius: 16px; box-shadow: 0 4px 12px rgba(0,0,0,0.06); overflow: hidden; border: 1px solid #e2e8f0;'>
                    <!-- Brand Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #1e3a8a 0%, #1d4ed8 100%); padding: 32px 30px; text-align: center;'>
                            <h1 style='margin: 0; color: #ffffff; font-size: 26px; font-weight: 800; letter-spacing: -0.5px;'>Onudhabon</h1>
                            <p style='margin: 6px 0 0 0; color: #93c5fd; font-size: 14px;'>Exploring Education & Community Learning</p>
                        </td>
                    </tr>
                    
                    <!-- Content Body -->
                    <tr>
                        <td style='padding: 36px 32px; color: #334155; font-size: 15px; line-height: 1.6;'>
                            <h2 style='margin: 0 0 16px 0; color: #0f172a; font-size: 20px; font-weight: 700;'>Hello, {displayName}!</h2>
                            
                            <p style='margin: 0 0 18px 0;'>We received a request to reset the password for your Onudhabon account. Click the button below to set a new password:</p>
                            
                            <!-- Call to Action Button -->
                            <table role='presentation' cellspacing='0' cellpadding='0' style='margin: 28px auto;'>
                                <tr>
                                    <td align='center' style='border-radius: 50px; background: linear-gradient(135deg, #1e3a8a 0%, #1d4ed8 100%);'>
                                        <a href='{resetUrl}' target='_blank' style='display: inline-block; padding: 14px 34px; color: #ffffff; font-size: 15px; font-weight: 600; text-decoration: none; border-radius: 50px;'>
                                            Reset Password
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <p style='margin: 24px 0 8px 0; font-size: 13px; color: #64748b;'>If the button above does not work, copy and paste this link into your web browser:</p>
                            <p style='margin: 0 0 20px 0; font-size: 12px; color: #2563eb; word-break: break-all;'>
                                <a href='{resetUrl}' style='color: #2563eb; text-decoration: underline;'>{resetUrl}</a>
                            </p>

                            <div style='background-color: #f1f5f9; border-left: 4px solid #3b82f6; border-radius: 4px; padding: 12px 16px; margin: 24px 0 12px 0;'>
                                <p style='margin: 0; font-size: 13px; color: #475569;'>
                                    <strong>Important:</strong> This password reset link will expire in <strong>1 hour</strong>. If you did not request a password reset, please ignore this email — your password will remain unchanged.
                                </p>
                            </div>

                            <p style='margin: 20px 0 0 0; font-size: 12px; color: #94a3b8;'>If you did not request this password reset, you can safely disregard this email.</p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8fafc; padding: 20px 32px; border-top: 1px solid #e2e8f0; text-align: center; color: #94a3b8; font-size: 12px;'>
                            &copy; {DateTime.UtcNow.Year} Onudhabon. All rights reserved.<br />
                            Dedicated to accessible education and volunteer empowerment.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task<bool> SendPasswordResetOtpEmailAsync(string toEmail, string recipientName, string otp)
        {
            var subject = $"Your Password Reset OTP: {otp} - Onudhabon";
            var displayName = string.IsNullOrWhiteSpace(recipientName) ? "User" : recipientName.Trim();

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>Password Reset OTP</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f8fafc; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='background-color: #f8fafc; padding: 40px 15px;'>
        <tr>
            <td align='center'>
                <table role='presentation' width='100%' style='max-width: 560px; background-color: #ffffff; border-radius: 16px; box-shadow: 0 4px 14px rgba(0,0,0,0.06); overflow: hidden; border: 1px solid #e2e8f0;'>
                    <!-- Brand Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #1e3a8a 0%, #1d4ed8 100%); padding: 32px 30px; text-align: center;'>
                            <h1 style='margin: 0; color: #ffffff; font-size: 26px; font-weight: 800; letter-spacing: -0.5px;'>Onudhabon</h1>
                            <p style='margin: 6px 0 0 0; color: #bfdbfe; font-size: 14px;'>Exploring Education & Community Learning</p>
                        </td>
                    </tr>
                    
                    <!-- Content Body -->
                    <tr>
                        <td style='padding: 36px 32px; color: #334155; font-size: 15px; line-height: 1.6;'>
                            <h2 style='margin: 0 0 14px 0; color: #0f172a; font-size: 20px; font-weight: 700;'>Hello, {displayName}!</h2>
                            
                            <p style='margin: 0 0 20px 0;'>We received a request to reset your password. Use the following One-Time Password (OTP) to verify your identity:</p>
                            
                            <!-- OTP Box -->
                            <div style='margin: 28px 0; text-align: center;'>
                                <div style='display: inline-block; font-size: 36px; letter-spacing: 10px; font-weight: 800; color: #1e3a8a; background: #eff6ff; padding: 18px 36px; border-radius: 14px; border: 2px dashed #3b82f6; font-family: ""Courier New"", Courier, monospace;'>
                                    {otp}
                                </div>
                                <div style='font-size: 12px; color: #64748b; margin-top: 8px;'>This code is valid for <strong>15 minutes</strong>. Do not share it with anyone.</div>
                            </div>

                            <div style='background-color: #f1f5f9; border-left: 4px solid #3b82f6; border-radius: 4px; padding: 12px 16px; margin: 20px 0;'>
                                <p style='margin: 0; font-size: 13px; color: #475569;'>
                                    <strong>Important:</strong> If you did not request a password reset, please ignore this email — your password will remain unchanged.
                                </p>
                            </div>

                            <p style='margin: 16px 0 0 0; font-size: 12px; color: #94a3b8;'>If you did not initiate this request, you can safely ignore this email.</p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8fafc; padding: 20px 32px; border-top: 1px solid #e2e8f0; text-align: center; color: #94a3b8; font-size: 12px;'>
                            &copy; {DateTime.UtcNow.Year} Onudhabon. All rights reserved.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var gmailUser = Environment.GetEnvironmentVariable("GMAIL_USER")
                ?? Environment.GetEnvironmentVariable("SMTP_EMAIL")
                ?? _configuration["Gmail:Email"]
                ?? _configuration["SmtpSettings:SenderEmail"]
                ?? string.Empty;

            var gmailAppPassword = Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD")
                ?? Environment.GetEnvironmentVariable("SMTP_PASSWORD")
                ?? _configuration["Gmail:AppPassword"]
                ?? _configuration["SmtpSettings:Password"]
                ?? string.Empty;

            var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST")
                ?? _configuration["SmtpSettings:Host"]
                ?? "smtp.gmail.com";

            var smtpPortStr = Environment.GetEnvironmentVariable("SMTP_PORT")
                ?? _configuration["SmtpSettings:Port"]
                ?? "587";

            int.TryParse(smtpPortStr, out int smtpPort);
            if (smtpPort <= 0) smtpPort = 587;

            gmailUser = gmailUser.Trim();
            gmailAppPassword = gmailAppPassword.Replace(" ", "").Trim();

            // Check if credentials are provided
            if (string.IsNullOrWhiteSpace(gmailUser) || string.IsNullOrWhiteSpace(gmailAppPassword))
            {
                _logger.LogWarning(
                    "Gmail SMTP credentials are not configured. Set GMAIL_USER and GMAIL_APP_PASSWORD in .env or appsettings.json. Verification email could not be sent over SMTP for: {ToEmail}.",
                    toEmail);
                return false;
            }

            try
            {
                using var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(gmailUser, "Onudhabon Platform");
                mailMessage.To.Add(new MailAddress(toEmail));
                mailMessage.Subject = subject;
                mailMessage.Body = htmlBody;
                mailMessage.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(gmailUser, gmailAppPassword),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Timeout = 15000 // 15 seconds timeout
                };

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Verification email successfully sent via Gmail SMTP to: {ToEmail}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via Gmail SMTP to {ToEmail}: {Message}", toEmail, ex.Message);
                return false;
            }
        }
    }
}
