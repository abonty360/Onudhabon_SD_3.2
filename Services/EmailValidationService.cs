using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Onudhabon.Services
{
    public class EmailValidationService : IEmailValidationService
    {
        private readonly ILogger<EmailValidationService> _logger;

        // Common temporary / disposable email domains that generate fake emails
        private static readonly HashSet<string> DisposableDomains = new(StringComparer.OrdinalIgnoreCase)
        {
            "tempmail.com", "temp-mail.org", "tempmail.net",
            "mailinator.com", "10minutemail.com", "guerrillamail.com",
            "trashmail.com", "yopmail.com", "getairmail.com",
            "throwawaymail.com", "fakemailgenerator.com", "dispostable.com",
            "sharklasers.com", "guerrillamailblock.com", "pokemail.net",
            "spam4.me", "nada.ltd", "mohmal.com", "burnermail.io",
            "inboxkitten.com", "crazymailing.com", "emailondeck.com"
        };

        public EmailValidationService(ILogger<EmailValidationService> logger)
        {
            _logger = logger;
        }

        public async Task<(bool Exists, string? ErrorMessage)> ValidateEmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return (false, "Email address cannot be empty.");
            }

            var cleanEmail = email.Trim().ToLowerInvariant();

            // 1. Format check
            var emailRegex = new Regex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$", RegexOptions.Compiled);
            if (!emailRegex.IsMatch(cleanEmail) || !cleanEmail.Contains('.'))
            {
                return (false, "Please provide a valid email address format (e.g., yourname@gmail.com).");
            }

            var parts = cleanEmail.Split('@');
            if (parts.Length != 2)
            {
                return (false, "Invalid email structure.");
            }

            var domain = parts[1];

            // 2. Block disposable/temporary burner email addresses
            if (DisposableDomains.Contains(domain))
            {
                return (false, "Temporary and disposable email addresses are not allowed. Please use your personal or institutional email.");
            }

            // 3. DNS check: Domain must resolve to active IP addresses
            try
            {
                var hostAddresses = await Dns.GetHostAddressesAsync(domain);
                if (hostAddresses == null || hostAddresses.Length == 0)
                {
                    return (false, $"The domain '@{domain}' does not exist or has no active mail server.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DNS resolution failed for domain {Domain}", domain);
                return (false, $"The domain '@{domain}' could not be resolved. Please enter a valid, active email address.");
            }

            // 4. Real-time SMTP Handshake Check (Primary check for Gmail and other major providers)
            try
            {
                string mxHost = domain.Equals("gmail.com", StringComparison.OrdinalIgnoreCase) || domain.Equals("googlemail.com", StringComparison.OrdinalIgnoreCase)
                    ? "gmail-smtp-in.l.google.com"
                    : domain;

                var smtpCheckResult = await CheckMailboxViaSmtpAsync(mxHost, cleanEmail);
                if (!smtpCheckResult.Exists)
                {
                    return (false, smtpCheckResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "SMTP existence check skipped or timed out for {Email}. Allowing fallback to domain validity.", cleanEmail);
            }

            return (true, null);
        }

        private async Task<(bool Exists, string? ErrorMessage)> CheckMailboxViaSmtpAsync(string mxHost, string targetEmail)
        {
            using var tcpClient = new TcpClient();
            
            // 4-second timeout to prevent slowing down registration
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(4));

            try
            {
                await tcpClient.ConnectAsync(mxHost, 25, cts.Token);
            }
            catch
            {
                // If port 25 cannot connect to this MX, we allow domain-level validation
                return (true, null);
            }

            using var stream = tcpClient.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            // Read banner
            var banner = await reader.ReadLineAsync(cts.Token);
            if (banner == null || !banner.StartsWith("220"))
            {
                return (true, null);
            }

            // HELO
            await writer.WriteLineAsync("HELO onudhabon.com");
            var heloResponse = await reader.ReadLineAsync(cts.Token);

            // MAIL FROM
            var senderEmail = Environment.GetEnvironmentVariable("GMAIL_USER") ?? "njtuli65@gmail.com";
            await writer.WriteLineAsync($"MAIL FROM:<{senderEmail}>");
            var mailFromResponse = await reader.ReadLineAsync(cts.Token);

            // RCPT TO
            await writer.WriteLineAsync($"RCPT TO:<{targetEmail}>");
            var rcptResponse = await reader.ReadLineAsync(cts.Token);

            // QUIT
            try
            {
                await writer.WriteLineAsync("QUIT");
            }
            catch { /* Ignore quit error */ }

            if (rcptResponse != null)
            {
                _logger.LogInformation("SMTP mailbox verification for {TargetEmail}: {Response}", targetEmail, rcptResponse);

                // Google & standard RFC SMTP responses for non-existent mailboxes:
                // 550: The email account that you tried to reach does not exist
                // 551, 552, 553: User unknown / mailbox not found
                if (rcptResponse.StartsWith("550") || rcptResponse.StartsWith("551") || rcptResponse.StartsWith("552") || rcptResponse.StartsWith("553")
                    || rcptResponse.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
                    || rcptResponse.Contains("user unknown", StringComparison.OrdinalIgnoreCase)
                    || rcptResponse.Contains("recipient rejected", StringComparison.OrdinalIgnoreCase)
                    || rcptResponse.Contains("mailbox unavailable", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "This email address does not exist on Google / mail provider. Please provide an active, existing email account to receive your OTP.");
                }
            }

            return (true, null);
        }
    }
}
