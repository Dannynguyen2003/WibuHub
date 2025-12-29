using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;

namespace WibuHub.Controllers
{
    public class ComicCategoriesController : Controller
    {
        private readonly StoryDbContext _context;

        public ComicCategoriesController(StoryDbContext context)
        {
            _context = context;
        }

        // GET: ComicCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Genres.ToListAsync());
        }

        // GET: ComicCategories/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comicCategory = await _context.Genres
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comicCategory == null)
            {
                return NotFound();
            }

            return View(comicCategory);
        }

        // GET: ComicCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ComicCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] ComicCategory comicCategory)
        {
            if (ModelState.IsValid)
            {
                comicCategory.Id = Guid.NewGuid();
                _context.Add(comicCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(comicCategory);
        }

        // GET: ComicCategories/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comicCategory = await _context.Genres.FindAsync(id);
            if (comicCategory == null)
            {
                return NotFound();
            }
            return View(comicCategory);
        }

        // POST: ComicCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name")] ComicCategory comicCategory)
        {
            if (id != comicCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comicCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComicCategoryExists(comicCategory.Id))
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
            return View(comicCategory);
        }

        // GET: ComicCategories/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comicCategory = await _context.Genres
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comicCategory == null)
            {
                return NotFound();
            }

            return View(comicCategory);
        }

        // POST: ComicCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var comicCategory = await _context.Genres.FindAsync(id);
            if (comicCategory != null)
            {
                _context.Genres.Remove(comicCategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComicCategoryExists(Guid id)
        {
            return _context.Genres.Any(e => e.Id == id);
        }
    }
}
