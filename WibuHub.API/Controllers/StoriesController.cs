using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.Service.Interface;

namespace WibuHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StoriesController : ControllerBase
    {
        private readonly IStoryService _storyService;
        private readonly ILogger<StoriesController> _logger;

        public StoriesController(IStoryService storyService, ILogger<StoriesController> logger)
        {
            _storyService = storyService;
            _logger = logger;
        }

        [HttpGet("liststory")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var stories = await _storyService.GetAllAsync();
            return Ok(stories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var story = await _storyService.GetByIdAsync(id);
            if (story == null)
            {
                return NotFound(new { message = "Không tìm thấy truyện" });
            }
            return Ok(story);
        }

        [HttpGet("newest")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNewestStories()
        {
            var stories = await _storyService.GetNewestStoriesAsync();
            return Ok(stories);
        }

        [HttpGet("top-views")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopViews()
        {
            try
            {
                var topStories = await _storyService.GetTopViewsAsync(5);
                return Ok(topStories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách Xem Nhiều Nhất");
                return StatusCode(500, new { message = "Lỗi hệ thống khi lấy dữ liệu" });
            }
        }

        // ĐÃ THÊM: API Lọc truyện theo thể loại
        [HttpGet("genre/{genreId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByGenre(Guid genreId)
        {
            var stories = await _storyService.GetStoriesByGenreAsync(genreId);
            return Ok(stories);
        }

        [HttpGet("search-suggest")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchSuggest(string q) // Sửa tham số thành 'q' cho khớp với Javascript
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(new List<object>());

            // Gọi qua Service thay vì gọi _context ở đây
            var suggestions = await _storyService.SearchSuggestAsync(q);

            return Ok(suggestions);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StoryDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var isSuccess = await _storyService.CreateAsync(request);
            if (isSuccess) return Ok(new { success = true, message = "Tạo truyện mới thành công" });

            return BadRequest(new { success = false, message = "Lỗi khi tạo truyện" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] StoryDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isSuccess = await _storyService.UpdateAsync(id, request);
            if (isSuccess) return Ok(new { success = true, message = "Cập nhật thành công" });

            return NotFound(new { success = false, message = "Không tìm thấy truyện để cập nhật" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var isSuccess = await _storyService.DeleteAsync(id);
            if (isSuccess) return Ok(new { success = true, message = "Đã xóa truyện" });

            return NotFound(new { success = false, message = "Không tìm thấy truyện để xóa" });
        }
    }
}