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
    public class CategoriesController : Controller
    {
        private readonly StoryDbContext _context;

        public CategoriesController(StoryDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Position)
                .ToListAsync();
            return View(categories);
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Stories)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryVM categoryVM)
        {
            if (ModelState.IsValid)
            {
                var countCategory = await _context.Categories.CountAsync();
                var existingCategories = await _context.Categories
                    .Where(c => c.Name == categoryVM.Name && !c.IsDeleted)
                    .ToListAsync();

                if (existingCategories.Count > 0)
                {
                    ModelState.AddModelError("Name", "Danh mục này đã tồn tại");
                    return View(categoryVM);
                }

                var category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = categoryVM.Name.Trim(),
                    Description = categoryVM.Description?.Trim(),
                    Position = countCategory + 1
                };
                
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoryVM);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryVM = await _context.Categories
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => new CategoryVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Position = c.Position
                })
                .SingleOrDefaultAsync();
            
            if (categoryVM == null)
            {
                return NotFound();
            }
            
            return View(categoryVM);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CategoryVM categoryVM)
        {
            if (id != categoryVM.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _context.Categories
                        .Where(c => c.Id.Equals(id) && !c.IsDeleted)
                        .SingleOrDefaultAsync();

                    if (category == null)
                    {
                        return NotFound();
                    }
                    
                    category.Name = categoryVM.Name.Trim();
                    category.Description = categoryVM.Description?.Trim();
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(categoryVM.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(categoryVM);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null && !category.IsDeleted)
            {
                var listCategory = await _context.Categories
                    .Where(c => c.Position > category.Position && !c.IsDeleted)
                    .ToListAsync();
                
                foreach (var cat in listCategory)
                {
                    cat.Position--;
                }
                
                category.IsDeleted = true;
                category.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(Guid id)
        {
            return _context.Categories.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}
