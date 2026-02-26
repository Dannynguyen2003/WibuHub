using Microsoft.AspNetCore.Mvc;
using WibuHub.Service.Interface;

namespace WibuHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        private readonly ICustomEmailSender _emailSender;
        public NotificationController(ICustomEmailSender emailSender)
        {
            _emailSender = emailSender;
        }
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification(string userEmail)
        {
            var subject = "Welcome to .NET 10!";
            var body = "<h1>Hello!</h1><p>This is a test email sent via MailKit.</p>";
            await _emailSender.SendEmailAsync(userEmail, subject, body);
            return Ok("Email sent successfully.");
        }
    }
}
