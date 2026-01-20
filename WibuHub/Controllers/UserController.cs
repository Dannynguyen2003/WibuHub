//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;
//using WibuHub.ApplicationCore.Entities.Identity;
//using static WibuHub.ApplicationCore.DTOs.Admin.Admin;
//// using WibuHub.Application.DTOs.Management;
//// using WibuHub.Application.Services.Management;


//namespace WibuHub.MVC.Areas.Admin.Controllers
//{
//    [Route("api/management/user")] // Route tách biệt cho khu vực quản trị
//    [ApiController]
//    // Chỉ cho phép nhóm vận hành truy cập, chặn Customer
//    [Authorize(Roles = "Admin,Uploader,Translator,Seller")]
//    public class UserController : ControllerBase
//    {
//        private readonly UserManager<StoryUser> _userManager;
//        //private readonly IContentManagementService _contentService; // Service chuyên quản lý nội dung
//        //private readonly IRevenueService _revenueService; // Service tính tiền/doanh thu

//        public UserController(
//            UserManager<StoryUser> userManager)
//            //IContentManagementService contentService
//            //IRevenueService revenueService)
//        {
//            _userManager = userManager;
//            //_contentService = contentService;
//            //_revenueService = revenueService;
//        }

//        // ==========================================================
//        // 1. DASHBOARD CÁ NHÂN (Tổng quan hiệu quả)
//        // ==========================================================

//        /// <summary>
//        /// Xem thống kê nhanh: Tổng view, tổng truyện đã up, tổng tiền kiếm được
//        /// </summary>
//        [HttpGet("dashboard-stats")]
//        public async Task<IActionResult> GetMyStats()
//        {
//            var userId = GetCurrentUserId();

//            // Lấy thống kê của chính User này
//            var stats = await _contentService.GetCreatorStatsAsync(userId);
//            // Return model: { TotalViews, TotalFollowers, TotalStories, MonthlyIncome }
//            return Ok(stats);
//        }

//        // ==========================================================
//        // 2. QUẢN LÝ TRUYỆN CỦA TÔI (My Uploads)
//        // ==========================================================

//        /// <summary>
//        /// Lấy danh sách truyện do chính user này đăng/sở hữu
//        /// </summary>
//        [HttpGet("my-stories")]
//        public async Task<IActionResult> GetMyStories([FromQuery] int page = 1)
//        {
//            var userId = GetCurrentUserId();
//            var stories = await _contentService.GetStoriesByCreatorAsync(userId, page);
//            return Ok(stories);
//        }

//        /// <summary>
//        /// Tạo truyện mới (Đăng ký tác phẩm)
//        /// </summary>
//        [HttpPost("my-stories")]
//        public async Task<IActionResult> CreateStory([FromBody] CreateStoryDto request)
//        {
//            var userId = GetCurrentUserId();

//            // Logic: Validate dữ liệu, map DTO, lưu xuống DB với OwnerId = userId
//            var newStoryId = await _contentService.CreateStoryAsync(userId, request);

//            return CreatedAtAction(nameof(GetMyStories), new { id = newStoryId }, newStoryId);
//        }

//        /// <summary>
//        /// Cập nhật truyện (Sửa tên, ảnh bìa, trạng thái Ongoing/Completed)
//        /// </summary>
//        [HttpPut("my-stories/{storyId}")]
//        public async Task<IActionResult> UpdateStory(int storyId, [FromBody] UpdateStoryDto request)
//        {
//            var userId = GetCurrentUserId();

//            // Cực kỳ quan trọng: Check xem truyện này có phải của User này không?
//            bool isOwner = await _contentService.IsStoryOwnerAsync(storyId, userId);
//            if (!isOwner) return Forbid("Bạn không có quyền sửa truyện này.");

//            await _contentService.UpdateStoryAsync(storyId, request);
//            return Ok("Cập nhật thành công.");
//        }

//        // ==========================================================
//        // 3. QUẢN LÝ CHAPTER & BÁN TRUYỆN
//        // ==========================================================

//        /// <summary>
//        /// Upload chapter mới cho truyện
//        /// </summary>
//        [HttpPost("my-stories/{storyId}/chapters")]
//        public async Task<IActionResult> UploadChapter(int storyId, [FromBody] UploadChapterDto request)
//        {
//            var userId = GetCurrentUserId();
//            if (!await _contentService.IsStoryOwnerAsync(storyId, userId)) return Forbid();

//            // request chứa: Tên chap, Nội dung (ảnh/text), Giá tiền (Coin)
//            await _contentService.AddChapterAsync(storyId, request);
//            return Ok("Upload chapter thành công.");
//        }

//        /// <summary>
//        /// Cài đặt giá bán / kiếm tiền cho Chapter
//        /// </summary>
//        [HttpPatch("my-stories/{storyId}/chapters/{chapterId}/monetization")]
//        public async Task<IActionResult> SetChapterPrice(int storyId, int chapterId, [FromBody] SetPriceDto request)
//        {
//            var userId = GetCurrentUserId();
//            // Validate quyền sở hữu...

//            // request.Price = 0 (Free) hoặc > 0 (Paid)
//            await _contentService.SetChapterPriceAsync(chapterId, request.Price);
//            return Ok("Đã cập nhật giá bán.");
//        }

//        // ==========================================================
//        // 4. DOANH THU & VÍ (Revenue - cho người bán/dịch)
//        // ==========================================================

//        /// <summary>
//        /// Xem lịch sử doanh thu (Ai mua truyện nào, ngày nào)
//        /// </summary>
//        //[HttpGet("revenue-history")]
//        //public async Task<IActionResult> GetRevenueHistory([FromQuery] DateRangeDto dateRange)
//        //{
//        //    var userId = GetCurrentUserId();
//        //    var history = await _revenueService.GetCreatorRevenueAsync(userId, dateRange.From, dateRange.To);
//        //    return Ok(history);
//        //}

//        /// <summary>
//        /// Yêu cầu rút tiền (Withdraw)
//        /// </summary>
//        //[HttpPost("withdraw-request")]
//        //public async Task<IActionResult> RequestWithdraw([FromBody] WithdrawDto request)
//        //{
//        //    var userId = GetCurrentUserId();
//        //    // Kiểm tra số dư khả dụng
//        //    var result = await _revenueService.CreateWithdrawRequestAsync(userId, request.Amount, request.BankInfo);

//        //    if (!result.Success) return BadRequest(result.Message);
//        //    return Ok("Đã gửi yêu cầu rút tiền.");
//        //}

//        // ==========================================================
//        // HELPER
//        // ==========================================================
//        private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
//    }
//}