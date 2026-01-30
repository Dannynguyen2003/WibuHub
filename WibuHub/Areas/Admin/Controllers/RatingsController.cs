using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;

namespace WibuHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class RatingsController : Controller
    {
        private readonly StoryDbContext _context;

        public RatingsController(StoryDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Ratings
        public async Task<IActionResult> Index()
        {
            var ratings = await _context.Ratings
                .Include(r => r.Story)
                .OrderByDescending(r => r.CreateDate)
                .ToListAsync();
            return View(ratings);
        }

        // GET: Admin/Ratings/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rating = await _context.Ratings
                .Include(r => r.Story)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }

        // GET: Admin/Ratings/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rating = await _context.Ratings
                .Include(r => r.Story)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }

        // POST: Admin/Ratings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating != null)
            {
                _context.Ratings.Remove(rating);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
