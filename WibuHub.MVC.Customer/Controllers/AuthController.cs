using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.Common.Contants;

namespace WibuHub.MVC.Customer.Controllers
{
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly SignInManager<StoryUser> _signInManager;
        private readonly UserManager<StoryUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<StoryRole> _roleManager;

        public AuthController(
          SignInManager<StoryUser> signInManager,
          UserManager<StoryUser> userManager,
          IEmailSender emailSender,
          RoleManager<StoryRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!TryValidateModel(request))
            {
                return Json(new AuthResponse(false, "Thông tin đăng nhập chưa hợp lệ."));
            }

            var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, request.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                return Json(new AuthResponse(true, null, GetReturnUrl(request.ReturnUrl)));
            }

            if (result.RequiresTwoFactor)
            {
                var url = Url.Page("/Account/LoginWith2fa", new { area = "Identity", returnUrl = GetReturnUrl(request.ReturnUrl) });
                return Json(new AuthResponse(false, "Cần xác thực 2 bước.", url));
            }

            if (result.IsLockedOut)
            {
                return Json(new AuthResponse(false, "Tài khoản bị khoá tạm thời. Vui lòng thử lại sau."));
            }

            return Json(new AuthResponse(false, "Email hoặc mật khẩu không đúng."));
        }

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!TryValidateModel(request))
            {
                return Json(new AuthResponse(false, "Thông tin đăng ký chưa hợp lệ."));
            }

            if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
            {
                return Json(new AuthResponse(false, "Mật khẩu xác nhận không khớp."));
            }

            var user = new StoryUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var message = string.Join(" ", result.Errors.Select(e => e.Description));
                return Json(new AuthResponse(false, string.IsNullOrWhiteSpace(message) ? "Đăng ký thất bại." : message));
            }

            if (!await _roleManager.RoleExistsAsync(AppConstants.Roles.Customer))
            {
                await _roleManager.CreateAsync(new StoryRole
                {
                    Name = AppConstants.Roles.Customer,
                    Description = "Khách hàng"
                });
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, AppConstants.Roles.Customer);
            if (!addRoleResult.Succeeded)
            {
                var message = string.Join(" ", addRoleResult.Errors.Select(e => e.Description));
                return Json(new AuthResponse(false, string.IsNullOrWhiteSpace(message) ? "Không thể gán quyền khách hàng." : message));
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var callbackUrl = Url.Page(
    "/Account/ConfirmEmail",
    pageHandler: null,
    values: new { area = "Identity", userId = user.Id, code = encodedToken, returnUrl = GetReturnUrl(request.ReturnUrl) },
    protocol: Request.Scheme);

            try
            {
                if (!string.IsNullOrWhiteSpace(callbackUrl))
                {
                    await _emailSender.SendEmailAsync(request.Email, "Xác nhận email", $"Vui lòng xác nhận tài khoản bằng cách <a href='{callbackUrl}'>bấm vào đây</a>.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI GỬI EMAIL]: {ex.Message}");
                return Json(new AuthResponse(true, "Đăng ký thành công nhưng hệ thống đang lỗi gửi mail xác nhận. Vui lòng thử đăng nhập!"));
            }

            return Json(new AuthResponse(true, "Đăng ký thành công. Vui lòng kiểm tra email đã xác nhận tài khoản.", GetReturnUrl(request.ReturnUrl)));
        }

        [HttpPost("forgot-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!TryValidateModel(request))
            {
                return Json(new AuthResponse(false, "Email chưa hợp lệ."));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null && await _userManager.IsEmailConfirmedAsync(user))
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                if (!string.IsNullOrWhiteSpace(callbackUrl))
                {
                    var encodedCallbackUrl = HtmlEncoder.Default.Encode(callbackUrl);
                    var emailBody = $@"<div style='font-family: Arial, sans-serif; line-height: 1.6;'>
<h2 style='color: #1f2937;'>Đặt lại mật khẩu</h2>
<p>Chúng tôi đã nhận yêu cầu đặt lại mật khẩu tài khoản WibuHub của bạn.</p>
<p><a href='{encodedCallbackUrl}' style='display: inline-block; padding: 10px 18px; background: #2563eb; color: #ffffff; text-decoration: none; border-radius: 6px;'>Đặt lại mật khẩu</a></p>
<p>Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.</p>
<p style='color: #6b7280; font-size: 12px;'>Liên kết chỉ có hiệu lực trong thời gian ngắn để đảm bảo an toàn.</p>
</div>";

                    await _emailSender.SendEmailAsync(
                        request.Email,
                        "Đặt lại mật khẩu WibuHub",
                        emailBody);
                }
            }

            return Json(new AuthResponse(true, "Nếu email tồn tại trong hệ thống, chúng tôi đã gửi liên kết đặt lại mật khẩu."));
        }

        private string GetReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            return Url.Action("Index", "Home", new { area = "" }) ?? "/";
        }

        public sealed record AuthResponse(bool Success, string? Message = null, string? RedirectUrl = null);

        public sealed class LoginRequest
        {
            [Required]
            [EmailAddress]
            public string Email { get; init; } = string.Empty;

            [Required]
            public string Password { get; init; } = string.Empty;

            public bool RememberMe { get; init; }

            public string? ReturnUrl { get; init; }
        }

        public sealed class RegisterRequest
        {
            [Required]
            [EmailAddress]
            public string Email { get; init; } = string.Empty;

            [Required]
            public string Password { get; init; } = string.Empty;

            [Required]
            public string ConfirmPassword { get; init; } = string.Empty;

            public string? ReturnUrl { get; init; }
        }

        public sealed class ForgotPasswordRequest
        {
            [Required]
            [EmailAddress]
            public string Email { get; init; } = string.Empty;
        }
    }
}
