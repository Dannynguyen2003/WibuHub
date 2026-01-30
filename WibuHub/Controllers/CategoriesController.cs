using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewComponents;
using WibuHub.MVC.ViewModels;

namespace WibuHub.MVC.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly StoryDbContext _context;

        public CategoriesController(StoryDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Name,Description")] CategoryVM categoryVM)
        public async Task<IActionResult> Create(CategoryVM categoryVM)
        {
            if (ModelState.IsValid)
            {
                //categoryVM.Id = Guid.NewGuid();
                //_context.Add(categoryVM);
                var countCategory = await _context.Categories.CountAsync();
                var categories = _context.Categories.Where(c => c.Name == categoryVM.Name).ToList();

                if (categories.Count > 0) return View(categoryVM);

                var category = new Category
                {
                    //Id = Guid.NewGuid(),
                    Name = categoryVM.Name.Trim(),
                    Description = categoryVM.Description?.Trim(),
                    Position = ++countCategory
                };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }
            return View(categoryVM);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var categoryVM = await _context.Categories.FindAsync(id);
            //if (categoryVM == null)
            //{
            //    return NotFound();
            //}
            var categoryVM = await _context.Categories
                .Where(c => c.Id == id)
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
                return BadRequest();
            }
            //return View(category);
            return View(nameof(Create), categoryVM);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CategoryVM categoryVM)
        {
            if (id != categoryVM.Id)
            {
                //return NotFound();
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //var category = await _context.Categories.FindAsync(id);
                    var category = await _context.Categories
                        //.Where(c => c.Id == id)
                        .Where(c => c.Id.Equals(id))
                        .SingleOrDefaultAsync();

                    if (category == null)
                    {
                        return BadRequest();
                    }
                    category.Name = categoryVM.Name.Trim();
                    category.Description = categoryVM.Description?.Trim();
                    //_context.Update(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Create));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(categoryVM.Id))
                    {
                        //return NotFound();
                        return BadRequest();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            //return View("Create", category);
            return View(nameof(Create), categoryVM);
        }

        // GET: Categories/Delete/5
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
