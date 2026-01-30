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
    public class ChaptersController : Controller
    {
        private readonly StoryDbContext _context;

        public ChaptersController(StoryDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Chapters
        public async Task<IActionResult> Index(Guid? storyId)
        {
            IQueryable<Chapter> query = _context.Chapters
                .Include(c => c.Story)
                .Where(c => !c.IsDeleted);

            if (storyId.HasValue)
            {
                query = query.Where(c => c.StoryId == storyId);
                ViewData["StoryId"] = storyId;
                var story = await _context.Stories.FindAsync(storyId);
                ViewData["StoryTitle"] = story?.Title;
            }

            var chapters = await query
                .OrderByDescending(c => c.CreateDate)
                .ToListAsync();
            
            return View(chapters);
        }

        // GET: Admin/Chapters/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (chapter == null)
            {
                return NotFound();
            }

            return View(chapter);
        }

        // GET: Admin/Chapters/Create
        public IActionResult Create(Guid? storyId)
        {
            var stories = _context.Stories.Where(s => !s.IsDeleted).OrderBy(s => s.Title);
            ViewData["StoryId"] = new SelectList(stories, "Id", "Title", storyId);
            
            if (storyId.HasValue)
            {
                ViewData["PreselectedStoryId"] = storyId;
            }
            
            return View();
        }

        // POST: Admin/Chapters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChapterVM chapterVM)
        {
            if (ModelState.IsValid)
            {
                var chapter = new Chapter
                {
                    Id = Guid.NewGuid(),
                    StoryId = chapterVM.StoryId,
                    Name = chapterVM.Name,
                    Number = chapterVM.Number,
                    Slug = chapterVM.Slug,
                    Content = chapterVM.Content,
                    ServerId = chapterVM.ServerId,
                    Price = chapterVM.Price,
                    Discount = chapterVM.Discount,
                    CreateDate = DateTime.UtcNow
                };

                _context.Add(chapter);
                
                // Update story's UpdateDate
                var story = await _context.Stories.FindAsync(chapterVM.StoryId);
                if (story != null)
                {
                    story.UpdateDate = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { storyId = chapterVM.StoryId });
            }

            var stories = _context.Stories.Where(s => !s.IsDeleted).OrderBy(s => s.Title);
            ViewData["StoryId"] = new SelectList(stories, "Id", "Title", chapterVM.StoryId);
            return View(chapterVM);
        }

        // GET: Admin/Chapters/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null || chapter.IsDeleted)
            {
                return NotFound();
            }

            var chapterVM = new ChapterVM
            {
                Id = chapter.Id,
                StoryId = chapter.StoryId,
                Name = chapter.Name,
                Number = chapter.Number,
                Slug = chapter.Slug,
                Content = chapter.Content,
                ServerId = chapter.ServerId,
                Price = chapter.Price,
                Discount = chapter.Discount
            };

            var stories = _context.Stories.Where(s => !s.IsDeleted).OrderBy(s => s.Title);
            ViewData["StoryId"] = new SelectList(stories, "Id", "Title", chapter.StoryId);
            return View(chapterVM);
        }

        // POST: Admin/Chapters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ChapterVM chapterVM)
        {
            if (id != chapterVM.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var chapter = await _context.Chapters.FindAsync(id);
                    if (chapter == null || chapter.IsDeleted)
                    {
                        return NotFound();
                    }

                    chapter.StoryId = chapterVM.StoryId;
                    chapter.Name = chapterVM.Name;
                    chapter.Number = chapterVM.Number;
                    chapter.Slug = chapterVM.Slug;
                    chapter.Content = chapterVM.Content;
                    chapter.ServerId = chapterVM.ServerId;
                    chapter.Price = chapterVM.Price;
                    chapter.Discount = chapterVM.Discount;

                    // Update story's UpdateDate
                    var story = await _context.Stories.FindAsync(chapterVM.StoryId);
                    if (story != null)
                    {
                        story.UpdateDate = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChapterExists(chapterVM.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { storyId = chapterVM.StoryId });
            }

            var stories = _context.Stories.Where(s => !s.IsDeleted).OrderBy(s => s.Title);
            ViewData["StoryId"] = new SelectList(stories, "Id", "Title", chapterVM.StoryId);
            return View(chapterVM);
        }

        // GET: Admin/Chapters/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            
            if (chapter == null)
            {
                return NotFound();
            }

            return View(chapter);
        }

        // POST: Admin/Chapters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter != null)
            {
                var storyId = chapter.StoryId;
                chapter.IsDeleted = true;
                chapter.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index), new { storyId = storyId });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ChapterExists(Guid id)
        {
            return _context.Chapters.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}
