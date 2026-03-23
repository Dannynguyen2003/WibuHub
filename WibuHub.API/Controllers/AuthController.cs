using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WibuHub.API.Models;
using WibuHub.ApplicationCore.Entities.Identity;

namespace WibuHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<StoryUser> _userManager;
        private readonly RoleManager<StoryRole> _roleManager;
        private readonly JwtSettings _jwtSettings;

        public AuthController(
        UserManager<StoryUser> userManager,
        RoleManager<StoryRole> roleManager,
        IOptions<JwtSettings> jwtOptions)
        {
            _userManager = userManager;
            _roleManager = roleManager; // Gán giá trị
            _jwtSettings = jwtOptions.Value;
        }

        //[AllowAnonymous]
        //[HttpPost("login")]
        //public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var user = await _userManager.FindByNameAsync(request.UserNameOrEmail);
        //    if (user == null)
        //    {
        //        user = await _userManager.FindByEmailAsync(request.UserNameOrEmail);
        //    }

        //    if (user == null)
        //    {
        //        return Unauthorized(new { message = "Thông tin đăng nhập không hợp lệ." });
        //    }

        //    var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        //    if (!passwordValid)
        //    {
        //        return Unauthorized(new { message = "Thông tin đăng nhập không hợp lệ." });
        //    }

        //    if (_userManager.Options.SignIn.RequireConfirmedEmail && !user.EmailConfirmed)
        //    {
        //        return Unauthorized(new { message = "Email chưa được xác nhận." });
        //    }

        //    var roles = await _userManager.GetRolesAsync(user);
        //    var claims = new List<Claim>
        //    {
        //        new(JwtRegisteredClaimNames.Sub, user.Id),
        //        new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
        //        new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        //        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //    };

        //    foreach (var role in roles)
        //    {
        //        claims.Add(new Claim(ClaimTypes.Role, role));
        //    }

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        //    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //    var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresMinutes);

        //    var token = new JwtSecurityToken(
        //        issuer: _jwtSettings.Issuer,
        //        audience: _jwtSettings.Audience,
        //        claims: claims,
        //        expires: expiresAt,
        //        signingCredentials: credentials);

        //    return Ok(new LoginResponse
        //    {
        //        AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
        //        ExpiresAt = expiresAt,
        //        UserId = user.Id,
        //        Email = user.Email,
        //        UserName = user.UserName,
        //        Roles = roles.ToArray()
        //    });
        //}
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(request.UserNameOrEmail);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(request.UserNameOrEmail);
            }

            if (user == null)
            {
                return Unauthorized(new { message = "Thông tin đăng nhập không hợp lệ." });
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                return Unauthorized(new { message = "Thông tin đăng nhập không hợp lệ." });
            }

            if (_userManager.Options.SignIn.RequireConfirmedEmail && !user.EmailConfirmed)
            {
                return Unauthorized(new { message = "Email chưa được xác nhận." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id),
        new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
        new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // ==========================================
            // [THÊM MỚI] KIỂM TRA VÀ THÊM CLAIM VIP
            // ==========================================
            bool isUserVip = user.VipExpireDate.HasValue && user.VipExpireDate.Value > DateTime.UtcNow;
            if (isUserVip)
            {
                claims.Add(new Claim("IsVip", "true"));
            }
            // ==========================================

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return Ok(new LoginResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expiresAt,
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = roles.ToArray(),

                // [THÊM MỚI TÙY CHỌN]: Trả về trạng thái VIP cho Frontend dễ xử lý UI
                // Nếu class LoginResponse của bạn chưa có 2 trường này thì bạn hãy thêm vào nhé!
                IsVip = isUserVip,
                VipExpireDate = user.VipExpireDate
            });
        }

        //[AllowAnonymous]
        //[HttpPost("rgister")]
        //public async Task<ActionResult<LoginResponse>> Register([FromBody] LoginRequest request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var user = await _userManager.FindByNameAsync(request.UserNameOrEmail);
        //    if (user == null)
        //    {
        //        user = await _userManager.FindByEmailAsync(request.UserNameOrEmail);
        //    }

        //    if (user == null)
        //    {
        //        return Unauthorized(new { message = "Thông tin đăng nhập không hợp lệ." });
        //    }

        //    var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        //    if (!passwordValid)
        //    {
        //        return Unauthorized(new { message = "Thông tin đăng nhập không hợp lệ." });
        //    }

        //    if (_userManager.Options.SignIn.RequireConfirmedEmail && !user.EmailConfirmed)
        //    {
        //        return Unauthorized(new { message = "Email chưa được xác nhận." });
        //    }

        //    var roles = await _userManager.GetRolesAsync(user);
        //    var claims = new List<Claim>
        //    {
        //        new(JwtRegisteredClaimNames.Sub, user.Id),
        //        new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
        //        new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        //        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //    };

        //    foreach (var role in roles)
        //    {
        //        claims.Add(new Claim(ClaimTypes.Role, role));
        //    }

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        //    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //    var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresMinutes);

        //    var token = new JwtSecurityToken(
        //        issuer: _jwtSettings.Issuer,
        //        audience: _jwtSettings.Audience,
        //        claims: claims,
        //        expires: expiresAt,
        //        signingCredentials: credentials);

        //    return Ok(new RegisterResponse
        //    {
        //        Success = true,
        //        Message = "Đăng ký thành công",
        //        UserId = user.Id,
        //        Email = user.Email,
        //        UserName = user.UserName,
        //        Roles = roles.ToArray()
        //    });
        //}
        [AllowAnonymous]
        [HttpPost("register")] // Đã sửa lại đúng chính tả
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request) // Dùng đúng model RegisterRequest
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Kiểm tra xác nhận mật khẩu
            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest(new { message = "Mật khẩu xác nhận không khớp." });
            }

            // 2. Kiểm tra xem email đã bị trùng trong Database chưa
            var existingUser = await _userManager.FindByEmailAsync(request.UserNameOrEmail);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email này đã được sử dụng." });
            }

            // 3. Khởi tạo đối tượng User mới
            var user = new StoryUser
            {
                UserName = request.UserNameOrEmail, // Hoặc cắt chuỗi lấy phần trước @ làm UserName
                Email = request.UserNameOrEmail
            };

            // 4. Lệnh quan trọng nhất: TẠO USER VÀO DATABASE
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                // Trả về chi tiết lỗi từ Identity (ví dụ: pass quá ngắn)
                var errorMsg = string.Join(" | ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = $"Đăng ký thất bại: {errorMsg}" });
            }

            // 1. Kiểm tra xem Role "Customer" đã có trong Database chưa
            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                // Nếu chưa có, tự động tạo mới Role này
                await _roleManager.CreateAsync(new StoryRole
                {
                    Name = "Customer",
                    Description = "Khách hàng"
                });
            }

            // 2. Gán Role cho người dùng vừa tạo
            var addRoleResult = await _userManager.AddToRoleAsync(user, "Customer");

            if (!addRoleResult.Succeeded)
            {
                // Ghi log lỗi nếu gán role thất bại (tùy chọn)
                Console.WriteLine("Lỗi khi gán Role Customer cho user.");
            }
            // ==========================================

            return Ok(new RegisterResponse
            {
                Success = true,
                Message = "Đăng ký thành công! Vui lòng đăng nhập.",
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = new[] { "Customer" } // Trả về mảng chứa tên Role cho Frontend biết luôn
            });
        }
    }
}
