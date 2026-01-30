using Microsoft.AspNetCore.Mvc;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.Service.Interface;

namespace WibuHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // URL: api/stories
    public class StoriesController : ControllerBase
    {
        private readonly IStoryService _storyService;
        private readonly ILogger<StoriesController> _logger;
        public StoriesController(IStoryService storyService, ILogger<StoriesController> logger)
        {
            _storyService = storyService;
            _logger = logger;
        }
        // GET: api/stories
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var stories = await _storyService.GetAllAsync();
            return Ok(stories);
        }
        // GET: api/stories/{id}
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
        // POST: api/stories
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StoryDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var isSuccess = await _storyService.CreateAsync(request);
            if (isSuccess)
            {
                return Ok(new { success = true, message = "Tạo truyện mới thành công" });
            }
            return BadRequest(new { success = false, message = "Lỗi khi tạo truyện (Có thể CategoryId không tồn tại)" });
        }
        // PUT: api/stories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] StoryDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var isSuccess = await _storyService.UpdateAsync(id, request);
            if (isSuccess)
            {
                return Ok(new { success = true, message = "Cập nhật thành công" });
            }
            return NotFound(new { success = false, message = "Không tìm thấy truyện để cập nhật" });
        }
        // DELETE: api/stories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var isSuccess = await _storyService.DeleteAsync(id);
            if (isSuccess)
            {
                return Ok(new { success = true, message = "Đã xóa truyện" });
            }
            return NotFound(new { success = false, message = "Không tìm thấy truyện để xóa" });
        }
    }
}