using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;

namespace WibuHub.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly StoryDbContext _context;
        private readonly StoryIdentityDbContext _identityContext;

        public OrdersController(StoryDbContext context, StoryIdentityDbContext identityContext)
        {
            _context = context;
            _identityContext = identityContext;
        }

        public async Task<IActionResult> Index(string? status)
        {
            await CleanupExpiredOrdersAsync();
            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.PaymentStatus != "Completed")
                .OrderByDescending(o => o.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.PaymentStatus == status);
            }

            var statuses = await _context.Orders
                .AsNoTracking()
                .Where(o => o.PaymentStatus != "Completed")
                .Select(o => o.PaymentStatus)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToListAsync();

            ViewBag.StatusOptions = statuses;
            ViewBag.SelectedStatus = status;

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> History()
        {
            await CleanupExpiredOrdersAsync();
            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.PaymentStatus == "Completed")
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Story)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var customer = await _identityContext.StoryUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == order.UserId);

            ViewBag.Customer = customer;

            return View(order);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,PaymentMethod,PaymentStatus,TransactionId,Note")] Order input)
        {
            if (id != input.Id)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = input.Id });
        }

        private async Task CleanupExpiredOrdersAsync()
        {
            var now = DateTime.UtcNow;
            var unpaidThreshold = now.AddDays(-30);
            var historyThreshold = now.AddMonths(-3);

            await _context.Orders
                .Where(o => (o.PaymentStatus == null || o.PaymentStatus != "Completed") && o.CreatedAt < unpaidThreshold)
                .ExecuteDeleteAsync();

            await _context.Orders
                .Where(o => o.PaymentStatus == "Completed" && o.CreatedAt < historyThreshold)
                .ExecuteDeleteAsync();
        }
    }
}
