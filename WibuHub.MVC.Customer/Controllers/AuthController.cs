using System.ComponentModel.DataAnnotations;
using System.Text;
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
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            if (!TryValidateModel(request))
            {
                return Json(new AuthResponse(false, "Thông tin ðãng nh?p chýa h?p l?."));
            }

            var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, request.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                return Json(new AuthResponse(true, null, GetReturnUrl(request.ReturnUrl)));
            }

            if (result.RequiresTwoFactor)
            {
                var url = Url.Page("/Account/LoginWith2fa", new { area = "Identity", returnUrl = GetReturnUrl(request.ReturnUrl) });
                return Json(new AuthResponse(false, "C?n xác th?c 2 bý?c.", url));
            }

            if (result.IsLockedOut)
            {
                return Json(new AuthResponse(false, "Tài kho?n b? khoá t?m th?i. Vui l?ng th? l?i sau."));
            }

            return Json(new AuthResponse(false, "Email ho?c m?t kh?u không ðúng."));
        }

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] RegisterRequest request)
        {
            if (!TryValidateModel(request))
            {
                return Json(new AuthResponse(false, "Thông tin ðãng k? chýa h?p l?."));
            }

            if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
            {
                return Json(new AuthResponse(false, "M?t kh?u xác nh?n không kh?p."));
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
                return Json(new AuthResponse(false, string.IsNullOrWhiteSpace(message) ? "Ðãng k? th?t b?i." : message));
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
                return Json(new AuthResponse(false, string.IsNullOrWhiteSpace(message) ? "Không th? gán quy?n khách hàng." : message));
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = encodedToken, returnUrl = GetReturnUrl(request.ReturnUrl) },
                protocol: Request.Scheme);

            if (!string.IsNullOrWhiteSpace(callbackUrl))
            {
                await _emailSender.SendEmailAsync(request.Email, "Xác nh?n email", $"Vui l?ng xác nh?n tài kho?n b?ng cách <a href='{callbackUrl}'>b?m vào ðây</a>.");
            }

            return Json(new AuthResponse(true, "Ðãng k? thành công. Vui l?ng ki?m tra email ð? xác nh?n tài kho?n.", GetReturnUrl(request.ReturnUrl)));
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
    }
}
