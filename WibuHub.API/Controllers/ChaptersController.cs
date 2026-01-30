using Microsoft.AspNetCore.Mvc;
using WibuHub.MVC.ViewModels;
using WibuHub.Service.Interface;

namespace WibuHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // URL: api/chapters
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

        // GET: api/chapters/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var chapter = await _chapterService.GetByIdAsync(id);
            if (chapter == null)
            {
                return NotFound(new { message = "Không tìm thấy chapter" });
            }
            return Ok(chapter);
        }

        // GET: api/chapters/story/{storyId}
        [HttpGet("story/{storyId}")]
        public async Task<IActionResult> GetByStoryId(Guid storyId)
        {
            var chapters = await _chapterService.GetByStoryIdAsync(storyId);
            return Ok(chapters);
        }

        // POST: api/chapters
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ChapterCreateVM request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isSuccess = await _chapterService.CreateAsync(request);

            if (isSuccess)
            {
                return Ok(new { success = true, message = "Tạo chapter mới thành công" });
            }

            return BadRequest(new { success = false, message = "Lỗi khi tạo chapter (Có thể StoryId không tồn tại hoặc Slug đã tồn tại)" });
        }

        // PUT: api/chapters/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ChapterCreateVM request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var isSuccess = await _chapterService.UpdateAsync(id, request);

            if (isSuccess)
            {
                return Ok(new { success = true, message = "Cập nhật thành công" });
            }

            return NotFound(new { success = false, message = "Không tìm thấy chapter để cập nhật hoặc Slug đã tồn tại" });
        }

        // DELETE: api/chapters/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var isSuccess = await _chapterService.DeleteAsync(id);

            if (isSuccess)
            {
                return Ok(new { success = true, message = "Đã xóa chapter" });
            }

            return NotFound(new { success = false, message = "Không tìm thấy chapter để xóa" });
        }
    }
}
