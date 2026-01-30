using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;

namespace WibuHub.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PaymentMethodsController : Controller
    {
        private readonly StoryDbContext _context;
        private readonly ILogger<PaymentMethodsController> _logger;

        public PaymentMethodsController(StoryDbContext context, ILogger<PaymentMethodsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: PaymentMethods
        public async Task<IActionResult> Index()
        {
            var paymentMethods = await _context.PaymentMethods
                .OrderBy(pm => pm.DisplayOrder)
                .ThenBy(pm => pm.Name)
                .ToListAsync();
            return View(paymentMethods);
        }

        // GET: PaymentMethods/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentMethod = await _context.PaymentMethods
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (paymentMethod == null)
            {
                return NotFound();
            }

            return View(paymentMethod);
        }

        // GET: PaymentMethods/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PaymentMethods/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Code,IsActive,LogoUrl,DisplayOrder,Description")] PaymentMethod paymentMethod)
        {
            if (ModelState.IsValid)
            {
                // Check if code already exists
                var existingCode = await _context.PaymentMethods
                    .AnyAsync(pm => pm.Code == paymentMethod.Code);
                
                if (existingCode)
                {
                    ModelState.AddModelError("Code", "Mã phương thức thanh toán đã tồn tại");
                    return View(paymentMethod);
                }

                _context.Add(paymentMethod);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm phương thức thanh toán '{paymentMethod.Name}' thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(paymentMethod);
        }

        // GET: PaymentMethods/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentMethod = await _context.PaymentMethods.FindAsync(id);
            if (paymentMethod == null)
            {
                return NotFound();
            }
            return View(paymentMethod);
        }

        // POST: PaymentMethods/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Code,IsActive,LogoUrl,DisplayOrder,Description")] PaymentMethod paymentMethod)
        {
            if (id != paymentMethod.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if code already exists (excluding current record)
                    var existingCode = await _context.PaymentMethods
                        .AnyAsync(pm => pm.Code == paymentMethod.Code && pm.Id != id);
                    
                    if (existingCode)
                    {
                        ModelState.AddModelError("Code", "Mã phương thức thanh toán đã tồn tại");
                        return View(paymentMethod);
                    }

                    _context.Update(paymentMethod);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã cập nhật phương thức thanh toán '{paymentMethod.Name}' thành công";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentMethodExists(paymentMethod.Id))
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
            return View(paymentMethod);
        }

        // GET: PaymentMethods/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentMethod = await _context.PaymentMethods
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (paymentMethod == null)
            {
                return NotFound();
            }

            return View(paymentMethod);
        }

        // POST: PaymentMethods/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paymentMethod = await _context.PaymentMethods.FindAsync(id);
            
            if (paymentMethod != null)
            {
                // Check if payment method is being used
                var isUsed = await _context.Transactions.AnyAsync(t => t.PaymentMethodId == id);
                
                if (isUsed)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa phương thức '{paymentMethod.Name}' vì đã có giao dịch sử dụng. Bạn có thể vô hiệu hóa thay vì xóa.";
                    return RedirectToAction(nameof(Index));
                }

                _context.PaymentMethods.Remove(paymentMethod);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa phương thức thanh toán '{paymentMethod.Name}' thành công";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: PaymentMethods/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var paymentMethod = await _context.PaymentMethods.FindAsync(id);
            
            if (paymentMethod == null)
            {
                return NotFound();
            }

            paymentMethod.IsActive = !paymentMethod.IsActive;
            await _context.SaveChangesAsync();

            var status = paymentMethod.IsActive ? "kích hoạt" : "vô hiệu hóa";
            TempData["SuccessMessage"] = $"Đã {status} phương thức thanh toán '{paymentMethod.Name}'";

            return RedirectToAction(nameof(Index));
        }

        private bool PaymentMethodExists(int id)
        {
            return _context.PaymentMethods.Any(e => e.Id == id);
        }
    }
}
