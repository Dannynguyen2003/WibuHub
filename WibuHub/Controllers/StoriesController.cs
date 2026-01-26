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
            var StoryDbContext = _context.Chapters.Include(s => s.Author).Include(s => s.Category);
            return View(await StoryDbContext.ToListAsync());
        }

        // GET: Stories/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var story = await _context.Chapters
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( StoryVM storyVM)
        {
            if (ModelState.IsValid)
            {
                var story = new Story
                {
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
                storyVM.Id = Guid.NewGuid();
                _context.Add(storyVM);
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

            var story = await _context.Chapters.FindAsync(id);
            if (story == null)
            {
                return NotFound();
            }
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", story.AuthorId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", story.CategoryId);
            return View(story);
        }

        // POST: Stories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                    _context.Update(storyVM);
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

            var story = await _context.Chapters
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
            var story = await _context.Chapters.FindAsync(id);
            if (story != null)
            {
                _context.Chapteres.Remove(story);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StoryExists(Guid id)
        {
            return _context.Chapters.Any(e => e.Id == id); 
        }
    }
}
