using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.DataLayer;
using WibuHub.Service.Interface;

namespace WibuHub.Controllers
{
    [Authorize]
    public class ChaptersController : Controller
    {
        private readonly IChapterService _chapterService;
        private readonly StoryDbContext _context;
        private readonly ILogger<ChaptersController> _logger;

        public ChaptersController(IChapterService chapterService, StoryDbContext context, ILogger<ChaptersController> logger)
        {
            _chapterService = chapterService;
            _context = context;
            _logger = logger;
        }

        // GET: Chapters
        public async Task<IActionResult> Index()
        {
            try
            {
                var chapters = await _chapterService.GetAllAsync();
                return View(chapters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chapters list");
                return View("Error");
            }
        }

        // GET: Chapters/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var chapter = await _chapterService.GetByIdAsync(id.Value);
                if (chapter == null)
                {
                    return NotFound();
                }

                return View(chapter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chapter details for ID: {Id}", id);
                return View("Error");
            }
        }

        // GET: Chapters/Create
        public IActionResult Create()
        {
            ViewData["StoryId"] = new SelectList(_context.Stories, "Id", "Title");
            return View();
        }

        // POST: Chapters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChapterDto chapterDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isSuccess = await _chapterService.CreateAsync(chapterDto);
                    if (isSuccess)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể tạo chương. Có thể StoryId không tồn tại.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating chapter");
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo chương.");
                }
            }
            ViewData["StoryId"] = new SelectList(_context.Stories, "Id", "Title", chapterDto.StoryId);
            return View(chapterDto);
        }

        // GET: Chapters/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var chapterDto = await _chapterService.GetByIdAsDtoAsync(id.Value);
                if (chapterDto == null)
                {
                    return NotFound();
                }
                ViewData["StoryId"] = new SelectList(_context.Stories, "Id", "Title", chapterDto.StoryId);
                return View(chapterDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chapter for edit: {Id}", id);
                return NotFound();
            }
        }

        // POST: Chapters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ChapterDto chapterDto)
        {
            if (id != chapterDto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var isSuccess = await _chapterService.UpdateAsync(chapterDto);
                    if (isSuccess)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể cập nhật chương.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating chapter: {Id}", id);
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật chương.");
                }
            }
            ViewData["StoryId"] = new SelectList(_context.Stories, "Id", "Title", chapterDto.StoryId);
            return View(chapterDto);
        }

        // GET: Chapters/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var chapter = await _chapterService.GetByIdAsync(id.Value);
                if (chapter == null)
                {
                    return NotFound();
                }

                return View(chapter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chapter for delete: {Id}", id);
                return NotFound();
            }
        }

        // POST: Chapters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var isSuccess = await _chapterService.DeleteAsync(id);
                if (!isSuccess)
                {
                    _logger.LogWarning("Failed to delete chapter: {Id}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chapter: {Id}", id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
