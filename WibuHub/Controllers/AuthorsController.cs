using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;
namespace WibuHub.Areas.Admin.Controllers
{
    //[Area("Admin")]
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
        public async Task<IActionResult> Create(AuthorVM authorVM)
        {
            if (ModelState.IsValid)
            {
                var existingAuthors = _context.Authors.Where(a => a.Name == authorVM.Name && !a.IsDeleted).ToList();
                if (existingAuthors.Count > 0) return View(authorVM);
                var author = new Author
                {
                    Name = authorVM.Name.Trim()
                };
                _context.Authors.Add(author);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }
            return View(authorVM);
        }
        // GET: Admin/Authors/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var authorVM = await _context.Authors
                .Where(a => a.Id == id && !a.IsDeleted)
                .Select(a => new AuthorVM
                {
                    Id = a.Id,
                    Name = a.Name
                })
                .SingleOrDefaultAsync();
            if (authorVM == null)
            {
                return BadRequest();
            }
            return View(nameof(Create), authorVM);
        }
        // POST: Admin/Authors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AuthorVM authorVM)
        {
            if (id != authorVM.Id)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var existingAuthor = await _context.Authors
                        .Where(a => a.Id.Equals(id) && !a.IsDeleted)
                        .SingleOrDefaultAsync();
                    if (existingAuthor == null)
                    {
                        return BadRequest();
                    }

                    existingAuthor.Name = authorVM.Name.Trim();
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Create));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if(!AuthorExists(authorVM.Id))
                    {
                        return BadRequest();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(nameof(Create), authorVM);
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
            if (author == null) return Json(new { isOK = false });
            author.IsDeleted = true;
            author.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Json(new { isOK = true });
        }
        // GET: Admin/Authors/Reload
        public async Task<IActionResult> Reload()
        {
            return ViewComponent("AuthorList");
        }
        private bool AuthorExists(Guid id)
        {
            return _context.Authors.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}