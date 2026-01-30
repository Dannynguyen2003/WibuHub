using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;

namespace WibuHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly StoryDbContext _context;

        public NotificationsController(StoryDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Notifications
        public async Task<IActionResult> Index()
        {
            var notifications = await _context.Notifications
                .OrderByDescending(n => n.CreateDate)
                .ToListAsync();
            return View(notifications);
        }

        // GET: Admin/Notifications/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(m => m.Id == id);

            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // GET: Admin/Notifications/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Admin/Notifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
