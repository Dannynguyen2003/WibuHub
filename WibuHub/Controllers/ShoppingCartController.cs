using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.DataLayer;
using WibuHub.MVC.ExtensionsMethod;
using WibuHub.MVC.ViewModels.ShoppingCart;

namespace WibuHub.MVC.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly StoryDbContext _context;
        private readonly string CartSessionKey = "CartSession";
        private readonly IHttpContextAccessor _httpContext;
        private readonly UserManager<StoryUser> _userManager;
        private readonly SignInManager<StoryUser> _signInManager;

        public ShoppingCartController(StoryDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObject<Cart>(CartSessionKey);
            return View(cart);
        }

        public async Task<IActionResult> AddToCart(Guid idChapter, int quantity)
        {
            // Logic to add the specified video to the shopping cart with the given quantity
            // This could involve checking if the cart exists, creating one if it doesn't,
            // and then adding or updating the item in the cart.
            // For now, just return a placeholder view.

            //=== Step 1: Lấy cart từ SESSION ===//
            var cart = HttpContext.Session.GetObject<Cart>(CartSessionKey);
            if (cart != null)
            {
                var cartItem = cart.Items.FirstOrDefault(item => item.ChapterId == idChapter);
                if (cartItem != null)
                {
                    // Video đã có trong giỏ hàng, cập nhật số lượng
                    cartItem.Quantity += quantity;
                }
                else
                {
                    // Video chưa có trong giỏ hàng, thêm mới
                    var chapter = await _context.Chapters.FindAsync(idChapter);
                    if (chapter != null)
                    {
                        cart.Items.Add(new CartItem
                        {
                            Id = Guid.NewGuid(),
                            CartId = cart.Id,
                            ChapterId = idChapter,
                            ChapterName = chapter.Name,
                            StoryTitle = chapter.Story?.Title ?? "",
                            Quantity = quantity,
                            Price = chapter.Price // Giá tiền có thể được lấy từ cơ sở dữ liệu hoặc dịch vụ khác
                        });
                    }
                }
            }
            else
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    //UserId = "ABC",
                    UserId = _httpContext.HttpContext?.User?.Identity?.Name,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Items = new List<CartItem>()
                };

                //cart.Items = new List<CartItem>();
                var chapter = await _context.Chapters.FindAsync(idChapter);
                if (chapter != null)
                {
                    cart.Items.Add(new CartItem
                    {
                        Id = Guid.NewGuid(),
                        CartId = cart.Id,
                        ChapterId = idChapter,
                        Quantity = quantity,
                        Price = chapter.Price // Giá tiền có thể được lấy từ cơ sở dữ liệu hoặc dịch vụ khác
                    });
                }
            }
            HttpContext.Session.SetObject(CartSessionKey, cart);

            var quantityReturn = cart.Items.Sum(it => it.Quantity);
            return Content(cart.Items.Count.ToString());
        }

        public async Task<IActionResult> UpdateQuantity(Guid idChapter, int quantity)
        {
            var cart = HttpContext.Session.GetObject<Cart>(CartSessionKey);
            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(x => x.ChapterId == idChapter);
                if (item != null)
                {
                    item.Quantity = quantity;
                    HttpContext.Session.SetObject<Cart>(CartSessionKey, cart);
                }
            }
            return Content(cart.Items.Count.ToString());
        }

        //public async Task<IActionResult> MakeThePayment()
        //{
        //    bool isOK = false;
        //    string message = "Chưa thực thi";

        //    var cart = HttpContext.Session.GetObject<Cart>(CartSessionKey);
        //    if (cart != null)
        //    {
        //        var items = cart.Items.ToList();
        //        if (items.Count > 0)
        //        {
        //            var newOrder = new Order
        //            {
        //                UserId = cart.UserId,
        //                Phone = "",
        //                Email = "",
        //                Note = "",
        //                ShippingAddress = "",
        //                Amount = 0,
        //                Tax = 0,
        //                TotalAmount = 0,
        //            };
        //            await _context.Orders.AddAsync(newOrder);
        //            var amount = 0.0M;
        //            foreach (var item in items)
        //            {
        //                var position = 1;
        //                var subAmount = item.Quantity * item.UnitPrice;
        //                amount += subAmount;
        //                var newOderDetail = new OrderDetail
        //                {
        //                    OrderId = newOrder.Id,
        //                    VideoId = item.VideoId,
        //                    Quantity = item.Quantity,
        //                    UnitPrice = item.UnitPrice,
        //                    Amount = subAmount,
        //                    Discount = 0,
        //                };
        //                await _context.OrderDetails.AddAsync(newOderDetail);
        //            }
        //            newOrder.Amount = amount;
        //            newOrder.Tax = amount * (decimal)0.1;
        //            newOrder.TotalAmount = newOrder.Amount + newOrder.Tax;
        //        }
        //    }

        //    return Json(new { isOK, message });
        //}

    }
}