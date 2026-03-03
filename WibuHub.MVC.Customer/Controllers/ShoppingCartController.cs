using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.DataLayer;
using WibuHub.MVC.Customer.ExtensionsMethod;
using WibuHub.MVC.ViewModels.ShoppingCart;

namespace WibuHub.MVC.Customer.Controllers
{
    public class ShoppingCartController : Controller
    {
        private const string CartSessionKey = "CartSession";
        private readonly StoryDbContext _context;

        public ShoppingCartController(StoryDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<Cart>(CartSessionKey) ?? new Cart();
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(Guid idChapter, int quantity = 1)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }

            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(c => c.Id == idChapter);
            if (chapter == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var cart = HttpContext.Session.GetObject<Cart>(CartSessionKey) ?? new Cart
            {
                Id = Guid.NewGuid(),
                UserId = User.Identity?.Name ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var item = cart.Items.FirstOrDefault(x => x.ChapterId == idChapter);
            if (item == null)
            {
                cart.Items.Add(new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ChapterId = chapter.Id,
                    ChapterName = chapter.Name,
                    StoryTitle = chapter.Story?.StoryName ?? chapter.StoryName,
                    Quantity = quantity,
                    Price = chapter.Price
                });
            }
            else
            {
                item.Quantity += quantity;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            HttpContext.Session.SetObject(CartSessionKey, cart);
            return RedirectToAction(nameof(Index));
        }
    }
}
