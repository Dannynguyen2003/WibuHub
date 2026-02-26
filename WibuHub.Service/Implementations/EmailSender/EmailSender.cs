using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using WibuHub.Service.EmailSender;

namespace WibuHub.MVC.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailSender> _logger;
        public EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            // 1. Set Sender and Recipient
            emailMessage.From.Add(new MailboxAddress(_emailSettings.SenderName,
           _emailSettings.SenderEmail));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            // 2. Set Content
            emailMessage.Subject = subject;

            // We use BodyBuilder to support HTML content cleanly
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = message,
                TextBody = "Please view this email in an HTML-compatible client." // Fallback
            };
            emailMessage.Body = bodyBuilder.ToMessageBody();
            // 3. Send using MailKit
            using var client = new SmtpClient();
            try
            {
                // Connect to the server
                // STARTTLS is generally recommended for port 587
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port,
               SecureSocketOptions.StartTls);
                // Authenticate
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                // Send
                await client.SendAsync(emailMessage);

                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {ToEmail}", toEmail);
                throw; // Re-throw or handle gracefully depending on your needs
            }
            finally
            {
                // Always disconnect cleanly
                await client.DisconnectAsync(true);
            }
        }
    }
}