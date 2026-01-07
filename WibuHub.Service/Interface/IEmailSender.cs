using Microsoft.AspNetCore.Identity.UI.Services;

namespace WibuHub.Service.Interface
{
    public interface ICustomEmailSender : IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}
