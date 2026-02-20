using Microsoft.AspNetCore.Mvc;
using WibuHub.MVC.Customer.ExtensionsMethod;
using WibuHub.MVC.Customer.Models.ShoppingCart;

namespace WibuHub.MVC.Customer.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "CustomerCartSession";

        public CartController()
        {
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<Cart>(CartSessionKey) ?? new Cart();
            return View("~/Views/ShoppingCart/Index.cshtml", cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(Guid storyId, string storyTitle)
        {
            var cart = HttpContext.Session.GetObject<Cart>(CartSessionKey) ?? new Cart();
            var item = cart.Items.FirstOrDefault(x => x.StoryId == storyId);
            if (item == null)
            {
                cart.Items.Add(new CartItem
                {
                    StoryId = storyId,
                    StoryTitle = storyTitle,
                    Quantity = 1
                });
            }
            else
            {
                item.Quantity++;
            }

            HttpContext.Session.SetObject(CartSessionKey, cart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(Guid storyId)
        {
            var cart = HttpContext.Session.GetObject<Cart>(CartSessionKey) ?? new Cart();
            var item = cart.Items.FirstOrDefault(x => x.StoryId == storyId);
            if (item != null)
            {
                cart.Items.Remove(item);
                HttpContext.Session.SetObject(CartSessionKey, cart);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
