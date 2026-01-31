using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;
namespace WibuHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class StoriesController : Controller
    {
        private readonly StoryDbContext _context;
        public StoriesController(StoryDbContext context)
        {
            _context = context;
        }
        // GET: Admin/Stories
        public async Task<IActionResult> Index()
        {
            var stories = await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Category)
                .Where(s => !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            return View(stories);
        }
        // GET: Admin/Stories/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var story = await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Category)
                .Include(s => s.Chapters)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            if (story == null)
            {
                return NotFound();
            }
            return View(story);
        }
        // GET: Admin/Stories/Create
        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Authors.Where(a => !a.IsDeleted), "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => !c.IsDeleted), "Id", "Name");
            return View();
        }
        // POST: Admin/Stories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoryVM storyVM)
        {
            if (ModelState.IsValid)
            {
                var story = new Story
                {
                    Id = Guid.NewGuid(),
                    StoryName = storyVM.Title.Trim(),
                    AlternativeName = storyVM.AlternativeName?.Trim(),
                    Description = storyVM.Description?.Trim(),
                    Slug = string.IsNullOrEmpty(storyVM.Slug)
                           ? GenerateSlug(storyVM.Title)
                           : storyVM.Slug.Trim(),
                    Status = storyVM.Status,
                    ViewCount = 0,
                    FollowCount = 0,
                    RatingScore = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow,
                    AuthorId = storyVM.AuthorId,
                    CategoryId = storyVM.CategoryId
                };
                _context.Add(story);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }

            ViewData["AuthorId"] = new SelectList(_context.Authors.Where(a => !a.IsDeleted), "Id", "Name", storyVM.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => !c.IsDeleted), "Id", "Name", storyVM.CategoryId);
            return View(storyVM);
        }

        private string GenerateSlug(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase)) return "";

            string str = phrase.ToLower().Trim();

            string[] vietnameseSigns = {
            "aAeEoOuUiIdDyY",
            "áàạảãâấầậẩẫăắằặẳẵ", "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ", "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ", "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ", "ÚÙỤỦŨƯỨỪỰỬỮ",
            "íìịỉĩ", "ÍÌỊỈĨ",
            "đ", "Đ",
            "ýỳỵỷỹ", "ÝỲỴỶỸ"
            };
            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                    str = str.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
            }

            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", "-").Trim();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"-+", "-");

            return str;
        }
        // GET: Admin/Stories/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var story = await _context.Stories.FindAsync(id);
            if (story == null || story.IsDeleted)
            {
                return NotFound();
            }
            var storyVM = new StoryVM
            {
                Id = story.Id,
                Title = story.StoryName,
                AlternativeName = story.AlternativeName,
                Description = story.Description,
                Slug = story.Slug,
                Status = story.Status,
                ViewCount = story.ViewCount,
                FollowCount = story.FollowCount,
                RatingScore = story.RatingScore,
                AuthorId = story.AuthorId,
                CategoryId = story.CategoryId
            };
            ViewData["AuthorId"] = new SelectList(_context.Authors.Where(a => !a.IsDeleted), "Id", "Name", story.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => !c.IsDeleted), "Id", "Name", story.CategoryId);
            return View(nameof(Create), storyVM);
        }
        // POST: Admin/Stories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, StoryVM storyVM)
        {
            if (id != storyVM.Id)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var story = await _context.Stories.FindAsync(id);
                    if (story == null || story.IsDeleted)
                    {
                        return BadRequest();
                    }
                    story.StoryName = storyVM.Title.Trim();
                    story.AlternativeName = storyVM.AlternativeName?.Trim();
                    story.Description = storyVM.Description?.Trim();
                    story.Slug = storyVM.Slug.Trim();
                    story.Status = storyVM.Status;
                    story.AuthorId = storyVM.AuthorId;
                    story.CategoryId = storyVM.CategoryId;
                    story.UpdateDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Create));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StoryExists(storyVM.Id))
                    {
                        return BadRequest();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["AuthorId"] = new SelectList(_context.Authors.Where(a => !a.IsDeleted), "Id", "Name", storyVM.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => !c.IsDeleted), "Id", "Name", storyVM.CategoryId);
            return View(nameof(Create), storyVM);
        }
        // GET: Admin/Stories/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var story = await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (story == null)
            {
                return NotFound();
            }
            return View(story);
        }
        // POST: Admin/Stories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var story = await _context.Stories.FindAsync(id);
            if (story == null) return Json(new { isOK = false });

            story.IsDeleted = true;
            story.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { isOK = true });
        }

        // GET: Admin/Stories/Reload
        public async Task<IActionResult> Reload()
        {
            return ViewComponent("StoryList");
        }

        private bool StoryExists(Guid id)
        {
            return _context.Stories.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}