using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;
using WibuHub.Service.EmailSender;
using WibuHub.Service.Interface;

namespace WibuHub.Service.Implementations.EmailSender
{
    public class MimeKitEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<MimeKitEmailSender> _logger;
        public MimeKitEmailSender(IOptions<EmailSettings> settings, ILogger<MimeKitEmailSender>
       logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            // 1. Create the MimeMessage (The "Container")
            var emailMessage = new MimeMessage();

            // From & To
            emailMessage.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;
            // 2. Construct the Body using BodyBuilder (The "Content")
            // BodyBuilder handles the complex MIME structure for you.
            var builder = new BodyBuilder();
            // Set the HTML content
            builder.HtmlBody = message;
            // Optional: Set a plain-text fallback for older email clients
            // You can strip HTML tags here or provide a custom summary.
            builder.TextBody = "Please view this email in a modern client to see the content.";
            // 4. Finalize the message body
            emailMessage.Body = builder.ToMessageBody();
            // 5. Send via MailKit (The "Transport")
            // MimeKit creates the message; MailKit sends it.
            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(emailMessage);
                _logger.LogInformation("Email sent to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email.");
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
