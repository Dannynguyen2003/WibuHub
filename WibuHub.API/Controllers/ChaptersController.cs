using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.Service.Interface;
using System.Security.Claims;

namespace WibuHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Bỏ [Authorize] ở đây để ai cũng gọi được API lấy danh sách
    public class ChaptersController : ControllerBase
    {
        private readonly IChapterService _chapterService;
        private readonly ILogger<ChaptersController> _logger;

        public ChaptersController(IChapterService chapterService, ILogger<ChaptersController> logger)
        {
            _chapterService = chapterService;
            _logger = logger;
        }

        // GET: api/chapters
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var chapters = await _chapterService.GetAllAsync();
            return Ok(chapters);
        }

        // GET: api/chapters/story/{storyId}
        [HttpGet("story/{storyId}")]
        public async Task<IActionResult> GetByStoryId(Guid storyId)
        {
            // Trả về danh sách chương (Nên ẩn Content/Images ở API này để nhẹ payload)
            var chapters = await _chapterService.GetByStoryIdAsync(storyId);
            return Ok(chapters);
        }

        // ==========================================
        // GET: api/chapters/{id} - API ĐỌC TRUYỆN CHÍNH
        // ==========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var chapter = await _chapterService.GetByIdAsync(id);
            if (chapter == null) return NotFound(new { message = "Không tìm thấy chapter" });

            // 1. Kiểm tra chương có MIỄN PHÍ không?
            if (chapter.IsFreeToRead)
            {
                return Ok(chapter); // Ai cũng đọc được
            }

            // --- TỪ ĐÂY TRỞ XUỐNG LÀ CHƯƠNG TRẢ PHÍ / VIP ---

            // 2. Nếu chương khóa, bắt buộc phải đăng nhập
            if (!User.Identity!.IsAuthenticated)
            {
                return StripContentAndReturn(chapter, 401, "Bạn cần đăng nhập để đọc chương này.");
            }

            // 3. Đã đăng nhập, check xem trong Token có Claim VIP không
            var isVipClaim = User.Claims.FirstOrDefault(c => c.Type == "IsVip")?.Value;
            bool isVip = isVipClaim == "true";

            if (!isVip)
            {
                // Gợi ý: Chỗ này sau này có thể check thêm: "User không có VIP nhưng đã dùng Point mua lẻ chương này chưa?"
                return StripContentAndReturn(chapter, 403, "Chương này dành riêng cho tài khoản VIP. Vui lòng nâng cấp!");
            }

            // 4. Nếu code chạy đến đây => User là VIP đang còn hạn
            return Ok(chapter);
        }

        // Các hàm Create, Update, Delete giữ nguyên nhưng THÊM [Authorize] vào
        [HttpPost]
        [Authorize] // Chỉ Admin/Uploader mới được tạo
        // LƯU Ý: Dùng [FromForm] thay vì [FromBody] để hỗ trợ upload file ảnh (multipart/form-data)
        public async Task<IActionResult> Create([FromForm] ChapterDto request)
        {
            if (request == null) return BadRequest("Dữ liệu không hợp lệ.");

            // Gọi hàm CreateAsync đã được tích hợp sẵn logic tạo Thông báo ở Service
            var isSuccess = await _chapterService.CreateAsync(request);

            if (isSuccess)
            {
                return Ok(new { success = true, message = "Thêm chapter mới thành công và đã gửi thông báo đến Followers!" });
            }

            return StatusCode(500, new { success = false, message = "Đã có lỗi xảy ra khi lưu chapter hoặc file ảnh." });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromForm] ChapterDto request)
        {
            if (request == null) return BadRequest("Dữ liệu không hợp lệ.");

            var isSuccess = await _chapterService.UpdateAsync(id, request);

            if (isSuccess) return Ok(new { success = true, message = "Cập nhật chapter thành công." });

            return BadRequest(new { success = false, message = "Không tìm thấy chapter hoặc cập nhật thất bại." });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var isSuccess = await _chapterService.DeleteAsync(id);

            if (isSuccess) return Ok(new { success = true, message = "Xóa chapter thành công." });

            return NotFound(new { success = false, message = "Không tìm thấy chapter để xóa." });
        }


        // --- HÀM HELPER BẢO MẬT ---
        // Xóa sạch nội dung truyện trước khi trả về lỗi 401/403 để tránh F12 ăn cắp data
        private IActionResult StripContentAndReturn(ChapterDto chapter, int statusCode, string message)
        {
            chapter.Content = null;
            chapter.ImageUrls = new List<string>();

            return StatusCode(statusCode, new
            {
                success = false,
                message = message,
                requiresVip = true,
                chapterInfo = chapter // Trả về info (tên chap, số chap) để UI render cái khung khóa 🔒
            });
        }
    }
}