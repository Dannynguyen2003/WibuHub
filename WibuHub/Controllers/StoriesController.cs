using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;
using WibuHub.Service.Interface;

namespace WibuHub.Controllers
{
    [Authorize]
    public class StoriesController : Controller
    {
        private readonly IStoryService _storyService;
        private readonly StoryDbContext _context;
        private readonly ILogger<StoriesController> _logger;

        public StoriesController(IStoryService storyService, StoryDbContext context, ILogger<StoriesController> logger)
        {
            _storyService = storyService;
            _context = context;
            _logger = logger;
        }

        // GET: Stories
        public async Task<IActionResult> Index()
        {
            try
            {
                var stories = await _storyService.GetAllAsync();
                return View(stories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stories list");
                return View("Error");
            }
        }

        // GET: Stories/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var story = await _storyService.GetByIdAsync(id.Value);
                if (story == null)
                {
                    return NotFound();
                }

                return View(story);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving story details for ID: {Id}", id);
                return View("Error");
            }
        }

        // GET: Stories/Create
        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Stories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoryVM storyVM)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isSuccess = await _storyService.CreateAsync(storyVM);
                    if (isSuccess)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể tạo truyện. Có thể CategoryId không tồn tại.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating story");
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo truyện.");
                }
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", storyVM.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", storyVM.CategoryId);
            return View(storyVM);
        }

        // GET: Stories/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var story = await _storyService.GetByIdAsync(id.Value);
                if (story == null)
                {
                    return NotFound();
                }

                // Chuyển đổi Entity sang ViewModel
                var storyVM = new StoryVM
                {
                    Id = story.Id,
                    Title = story.Title,
                    AlternativeName = story.AlternativeName,
                    Description = story.Description,
                    Thumbnail = story.Thumbnail,
                    Status = story.Status,
                    ViewCount = story.ViewCount,
                    FollowCount = story.FollowCount,
                    RatingScore = story.RatingScore,
                    AuthorId = story.AuthorId,
                    CategoryId = story.CategoryId
                };

                ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", story.AuthorId);
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", story.CategoryId);

                return View(storyVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading story for edit: {Id}", id);
                return NotFound();
            }
        }

        // POST: Stories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, StoryVM storyVM)
        {
            if (id != storyVM.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var isSuccess = await _storyService.UpdateAsync(id, storyVM);
                    if (isSuccess)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể cập nhật truyện. Có thể truyện không tồn tại hoặc CategoryId không hợp lệ.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating story: {Id}", id);
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật truyện.");
                }
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", storyVM.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", storyVM.CategoryId);
            return View(storyVM);
        }

        // GET: Stories/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var story = await _storyService.GetByIdAsync(id.Value);
                if (story == null)
                {
                    return NotFound();
                }

                return View(story);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading story for delete: {Id}", id);
                return NotFound();
            }
        }

        // POST: Stories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var isSuccess = await _storyService.DeleteAsync(id);
                if (!isSuccess)
                {
                    _logger.LogWarning("Failed to delete story: {Id}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting story: {Id}", id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}