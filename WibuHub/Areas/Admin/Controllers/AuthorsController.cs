using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;

namespace WibuHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AuthorsController : Controller
    {
        private readonly StoryDbContext _context;

        public AuthorsController(StoryDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Authors
        public async Task<IActionResult> Index()
        {
            var authors = await _context.Authors
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.Name)
                .ToListAsync();
            return View(authors);
        }

        // GET: Admin/Authors/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .Include(a => a.Stories)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // GET: Admin/Authors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Authors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Author author)
        {
            if (ModelState.IsValid)
            {
                author.Id = Guid.NewGuid();
                _context.Add(author);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Admin/Authors/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors.FindAsync(id);
            if (author == null || author.IsDeleted)
            {
                return NotFound();
            }
            return View(author);
        }

        // POST: Admin/Authors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name")] Author author)
        {
            if (id != author.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAuthor = await _context.Authors.FindAsync(id);
                    if (existingAuthor == null || existingAuthor.IsDeleted)
                    {
                        return NotFound();
                    }
                    
                    existingAuthor.Name = author.Name;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(author.Id))
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
            return View(author);
        }

        // GET: Admin/Authors/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .Include(a => a.Stories)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // POST: Admin/Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author != null)
            {
                author.IsDeleted = true;
                author.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AuthorExists(Guid id)
        {
            return _context.Authors.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}
