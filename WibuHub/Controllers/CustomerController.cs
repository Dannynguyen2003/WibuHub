using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WibuHub.ApplicationCore.Entities.Identity;

namespace WibuHub.MVC.Admin.Controllers
{
    [Route("api/customer")]
    [ApiController]
    [Authorize] // Bắt buộc đăng nhập mới gọi được
    public class CustomerController : ControllerBase
    {
        private readonly UserManager<StoryUser> _userManager;
        private readonly ICustomerService _customerService; // Xử lý nghiệp vụ đọc truyện/follow
        private readonly IWalletService _walletService;     // Xử lý nghiệp vụ tiền nong

        public CustomerController(
            UserManager<StoryUser> userManager,
            ICustomerService customerService,
            IWalletService walletService)
        {
            _userManager = userManager;
            _customerService = customerService;
            _walletService = walletService;
        }

        // ==========================================================
        // NHÓM 1: HỒ SƠ NGƯỜI ĐỌC (PROFILE)
        // ==========================================================

        /// <summary>
        /// Lấy thông tin cá nhân & Số dư ví
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();

            // Lấy thông tin cơ bản
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Tài khoản không tồn tại.");

            // Lấy số dư ví hiện tại
            var balance = await _walletService.GetBalanceAsync(userId);

            var profileDto = new
            {
                user.Id,
                user.Fullname,
                user.AvatarUrl,
                user.Email,
                Level = user.Level,     // Cấp độ tu tiên
                CoinBalance = balance,  // Số xu hiện có (quan trọng với người mua)
                IsVip = user.IsVip      // Trạng thái VIP (nếu có)
            };

            return Ok(profileDto);
        }

        /// <summary>
        /// Cập nhật hồ sơ (Avatar, Nickname)
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileDto request)
        {
            var user = await _userManager.GetUserAsync(User);

            user.Fullname = request.Fullname;
            user.AvatarUrl = request.AvatarUrl;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok("Cập nhật hồ sơ thành công.");
        }

        // ==========================================================
        // NHÓM 2: TỦ TRUYỆN (LIBRARY / BOOKMARKS)
        // ==========================================================

        /// <summary>
        /// Lấy danh sách truyện đang theo dõi
        /// </summary>
        [HttpGet("library")]
        public async Task<IActionResult> GetLibrary([FromQuery] int page = 1, [FromQuery] string? status = null)
        {
            // status: Reading, Completed, OnHold...
            var userId = GetCurrentUserId();
            var library = await _customerService.GetFollowedSeriesAsync(userId, page, status);
            return Ok(library);
        }

        /// <summary>
        /// Theo dõi / Bỏ theo dõi truyện
        /// </summary>
        [HttpPost("library/toggle")]
        public async Task<IActionResult> ToggleFollow([FromBody] ToggleFollowDto request)
        {
            var userId = GetCurrentUserId();
            // Hàm này tự động check: Nếu chưa follow thì thêm, nếu có rồi thì xóa (hoặc update status)
            var result = await _customerService.ToggleFollowAsync(userId, request.StoryId);
            return Ok(new { Message = result ? "Đã theo dõi" : "Đã bỏ theo dõi", IsFollowing = result });
        }

        // ==========================================================
        // NHÓM 3: LỊCH SỬ ĐỌC (READING HISTORY)
        // ==========================================================

        /// <summary>
        /// Lấy danh sách truyện vừa đọc gần đây
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetReadingHistory([FromQuery] int limit = 20)
        {
            var userId = GetCurrentUserId();
            var history = await _customerService.GetReadingHistoryAsync(userId, limit);
            return Ok(history);
        }

        /// <summary>
        /// Lưu tiến độ đọc (Gọi khi user mở 1 chapter)
        /// </summary>
        [HttpPost("history")]
        public async Task<IActionResult> SaveReadingProgress([FromBody] ReadingProgressDto request)
        {
            var userId = GetCurrentUserId();
            await _customerService.SaveProgressAsync(userId, request.StoryId, request.ChapterId);
            return Ok();
        }

        // ==========================================================
        // NHÓM 4: VÍ & MUA TRUYỆN (WALLET & PURCHASE)
        // ==========================================================

        /// <summary>
        /// Mua (Mở khóa) một Chapter tính phí
        /// </summary>
        [HttpPost("buy-chapter")]
        public async Task<IActionResult> BuyChapter([FromBody] BuyChapterDto request)
        {
            var userId = GetCurrentUserId();

            // Gọi Service xử lý giao dịch:
            // 1. Check số dư đủ không?
            // 2. Trừ tiền user -> Cộng tiền hệ thống/uploader
            // 3. Lưu vào bảng PurchasedChapters để lần sau vào đọc không mất tiền nữa
            var result = await _walletService.PurchaseChapterAsync(userId, request.ChapterId);

            if (!result.Success)
                return BadRequest(new { Error = result.Message }); // "Số dư không đủ"

            return Ok(new { Message = "Mở khóa thành công", NewBalance = result.RemainingBalance });
        }

        /// <summary>
        /// Xem lịch sử giao dịch (Nạp tiền / Mua truyện)
        /// </summary>
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactionHistory([FromQuery] int page = 1)
        {
            var userId = GetCurrentUserId();
            var transactions = await _walletService.GetTransactionHistoryAsync(userId, page);
            return Ok(transactions);
        }

        // ==========================================================
        // HELPER
        // ==========================================================
        private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
