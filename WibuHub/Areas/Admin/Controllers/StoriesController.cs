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
                .OrderByDescending(s => s.DateCreated)
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

                _context.Add(story);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["AuthorId"] = new SelectList(_context.Authors.Where(a => !a.IsDeleted), "Id", "Name", storyVM.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => !c.IsDeleted), "Id", "Name", storyVM.CategoryId);
            return View(storyVM);
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

            ViewData["AuthorId"] = new SelectList(_context.Authors.Where(a => !a.IsDeleted), "Id", "Name", story.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => !c.IsDeleted), "Id", "Name", story.CategoryId);
            return View(storyVM);
        }

        // POST: Admin/Stories/Edit/5
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
                    var story = await _context.Stories.FindAsync(id);
                    if (story == null || story.IsDeleted)
                    {
                        return NotFound();
                    }

                    story.Title = storyVM.Title;
                    story.AlternativeName = storyVM.AlternativeName;
                    story.Description = storyVM.Description;
                    story.Thumbnail = storyVM.Thumbnail;
                    story.Status = storyVM.Status;
                    story.AuthorId = storyVM.AuthorId;
                    story.CategoryId = storyVM.CategoryId;
                    story.UpdateDate = DateTime.UtcNow;

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
            
            ViewData["AuthorId"] = new SelectList(_context.Authors.Where(a => !a.IsDeleted), "Id", "Name", storyVM.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => !c.IsDeleted), "Id", "Name", storyVM.CategoryId);
            return View(storyVM);
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
            if (story != null)
            {
                story.IsDeleted = true;
                story.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StoryExists(Guid id)
        {
            return _context.Stories.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}
