using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;

namespace WibuHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class FollowsController : Controller
    {
        private readonly StoryDbContext _context;

        public FollowsController(StoryDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Follows
        public async Task<IActionResult> Index()
        {
            var follows = await _context.Follows
                .Include(f => f.Story)
                .OrderByDescending(f => f.FollowDate)
                .ToListAsync();
            return View(follows);
        }

        // GET: Admin/Follows/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var follow = await _context.Follows
                .Include(f => f.Story)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (follow == null)
            {
                return NotFound();
            }

            return View(follow);
        }

        // GET: Admin/Follows/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var follow = await _context.Follows
                .Include(f => f.Story)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (follow == null)
            {
                return NotFound();
            }

            return View(follow);
        }

        // POST: Admin/Follows/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var follow = await _context.Follows.FindAsync(id);
            if (follow != null)
            {
                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
