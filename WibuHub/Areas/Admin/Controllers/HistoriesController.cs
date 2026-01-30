using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;

namespace WibuHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HistoriesController : Controller
    {
        private readonly StoryDbContext _context;

        public HistoriesController(StoryDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Histories
        public async Task<IActionResult> Index()
        {
            var histories = await _context.Histories
                .Include(h => h.Story)
                .Include(h => h.Chapter)
                .OrderByDescending(h => h.ReadDate)
                .ToListAsync();
            return View(histories);
        }

        // GET: Admin/Histories/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var history = await _context.Histories
                .Include(h => h.Story)
                .Include(h => h.Chapter)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (history == null)
            {
                return NotFound();
            }

            return View(history);
        }

        // GET: Admin/Histories/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var history = await _context.Histories
                .Include(h => h.Story)
                .Include(h => h.Chapter)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (history == null)
            {
                return NotFound();
            }

            return View(history);
        }

        // POST: Admin/Histories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var history = await _context.Histories.FindAsync(id);
            if (history != null)
            {
                _context.Histories.Remove(history);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
