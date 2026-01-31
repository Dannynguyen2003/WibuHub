using FileTypeChecker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;
using static System.Net.Mime.MediaTypeNames;

namespace WibuHub.Controllers
{
    [Authorize]
    public class ChaptersController : Controller
    {
        private readonly StoryDbContext _context;

        public ChaptersController(StoryDbContext context)
        {
            _context = context;
        }

        // GET: Chapters
        public async Task<IActionResult> Index()
        {
            var StoryDbContext = _context.Chapters.Include(c => c.Story);
            return View(await StoryDbContext.ToListAsync());
        }

        // GET: Chapters/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chapter == null)
            {
                return NotFound();
            }

            return View(chapter);
        }

        // GET: Chapters/Create
        public IActionResult Create()
        {
            ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName");
            return View();
        }

        // POST: Chapters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChapterVM chapterVM)
        {
            if (ModelState.IsValid)
            {
                var chapter = new Chapter
                {
                    StoryId = chapterVM.StoryId,
                    Name = chapterVM.Name.Trim(),
                    ChapterNumber = chapterVM.ChapterNumber,
                    Slug = string.IsNullOrEmpty(chapterVM.Slug)
                           ? GenerateSlug(chapterVM.Name)
                           : chapterVM.Slug.Trim(),
                    ViewCount = 0,
                    Content = chapterVM.Content?.Trim(),
                    ServerId = chapterVM.ServerId,
                    CreatedAt = DateTime.UtcNow,
                    Price = chapterVM.Price,
                    Discount = chapterVM.Discount,
                };
                await _context.Chapters.AddAsync(chapter);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }
            ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapterVM.StoryId);
            return View(chapterVM);
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

        // GET: Chapters/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null)
            {
                return NotFound();
            }
            var chapterVM = new ChapterVM
            {
                Id = chapter.Id,
                StoryId = chapter.StoryId,
                Name = chapter.Name,
                ChapterNumber = chapter.ChapterNumber,
                Slug = chapter.Slug,
                ViewCount = chapter.ViewCount,
                Content = chapter.Content,
                ServerId = chapter.ServerId,
                CreatedAt = chapter.CreatedAt,
                Price = chapter.Price,
                Discount = chapter.Discount
            };
            ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapter.StoryId);
            return View(nameof(Create), chapterVM);
        }

        // POST: Chapters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ChapterVM chapterVM)
        {
            if (id != chapterVM.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var chapter = await _context.Chapters.FindAsync(id);
                    if (chapter == null)
                    {
                        return BadRequest();
                    }
                    chapter.StoryId = chapterVM.StoryId;
                    chapter.Name = chapterVM.Name.Trim();
                    chapter.ChapterNumber = chapterVM.ChapterNumber;
                    chapter.Slug = chapterVM.Slug.Trim();
                    chapter.Content = chapterVM.Content?.Trim();
                    chapter.ServerId = chapterVM.ServerId;
                    chapter.Price = chapterVM.Price;
                    chapter.Discount = chapterVM.Discount;
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Create));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChapterExists(chapterVM.Id))
                    {
                        return BadRequest();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewData["StoryId"] = new SelectList(_context.Stories.Where(s => !s.IsDeleted), "Id", "StoryName", chapterVM.StoryId);
            return View(nameof(Create), chapterVM);
        }

        // GET: Chapters/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chapter == null)
            {
                return NotFound();
            }

            return View(chapter);
        }

        // POST: Chapters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null) return Json(new { isOK = false });

            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();

            return Json(new { isOK = true });
        }

        // GET: Chapters/Reload
        public async Task<IActionResult> Reload()
        {
            return ViewComponent("ChapterList");
        }

        private bool ChapterExists(Guid id)
        {
            return _context.Chapters.Any(e => e.Id == id);
        }
    }
}
