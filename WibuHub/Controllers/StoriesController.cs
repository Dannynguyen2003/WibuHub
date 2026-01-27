using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;

namespace WibuHub.Controllers
{
    [Authorize]
    public class StoriesController : Controller
    {
        private readonly StoryDbContext _context;

        public StoriesController(StoryDbContext context)
        {
            _context = context;
        }

        // GET: Stories
        public async Task<IActionResult> Index()
        {
            // FIX: Đổi tên biến local cho đỡ nhầm lẫn với DbContext type
            // FIX: Giả định DbSet tên là Stories (thay vì Chapters)
            var stories = _context.Stories.Include(s => s.Author).Include(s => s.Category);
            return View(await stories.ToListAsync());
        }

        // GET: Stories/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var story = await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (story == null)
            {
                return NotFound();
            }

            return View(story);
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
                // Mapping từ ViewModel sang Entity
                var story = new Story
                {
                    Id = Guid.NewGuid(), // Tạo ID mới cho Entity
                    Title = storyVM.Title,
                    AlternativeName = storyVM.AlternativeName,
                    Description = storyVM.Description,
                    Thumbnail = storyVM.Thumbnail,
                    Status = storyVM.Status,
                    ViewCount = storyVM.ViewCount,
                    FollowCount = storyVM.FollowCount,
                    RatingScore = storyVM.RatingScore,
                    DateCreated = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow,
                    AuthorId = storyVM.AuthorId,
                    CategoryId = storyVM.CategoryId
                };

                // FIX: Thêm Entity (story) vào context, KHÔNG phải thêm ViewModel (storyVM)
                _context.Add(story);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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

            var story = await _context.Stories.FindAsync(id);
            if (story == null)
            {
                return NotFound();
            }

            // FIX: Chuyển đổi Entity sang ViewModel để tránh lỗi Mismatch Model ở View
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

            // Trả về ViewModel
            return View(storyVM);
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
                    // FIX: Lấy Entity gốc từ DB lên
                    var storyToUpdate = await _context.Stories.FindAsync(id);
                    if (storyToUpdate == null)
                    {
                        return NotFound();
                    }

                    // FIX: Cập nhật các trường từ ViewModel vào Entity
                    storyToUpdate.Title = storyVM.Title;
                    storyToUpdate.AlternativeName = storyVM.AlternativeName;
                    storyToUpdate.Description = storyVM.Description;
                    storyToUpdate.Thumbnail = storyVM.Thumbnail;
                    storyToUpdate.Status = storyVM.Status;
                    storyToUpdate.ViewCount = storyVM.ViewCount;
                    storyToUpdate.FollowCount = storyVM.FollowCount;
                    storyToUpdate.RatingScore = storyVM.RatingScore;
                    storyToUpdate.AuthorId = storyVM.AuthorId;
                    storyToUpdate.CategoryId = storyVM.CategoryId;
                    storyToUpdate.UpdateDate = DateTime.UtcNow; // Cập nhật ngày sửa

                    // Lưu thay đổi
                    _context.Update(storyToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StoryExists(storyVM.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
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

            var story = await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (story == null)
            {
                return NotFound();
            }

            return View(story);
        }

        // POST: Stories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var story = await _context.Stories.FindAsync(id);
            if (story != null)
            {
                _context.Stories.Remove(story);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StoryExists(Guid id)
        {
            return _context.Stories.Any(e => e.Id == id);
        }
    }
}