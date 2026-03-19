using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Configuration;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.MVC.Customer.ExtensionsMethod;
using WibuHub.MVC.Customer.ViewModels.ShoppingCart;
using WibuHub.MVC.ViewModels.ShoppingCart;
using WibuHub.Service.Interface;

namespace WibuHub.MVC.Customer.Controllers
{
    public class ShoppingCartController : Controller
    {
        private const string CartSessionKey = "CartSession";
        private readonly StoryDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MomoSettings _momoSettings;

        public ShoppingCartController(
            StoryDbContext context,
            IPaymentService paymentService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<MomoSettings> momoSettings)
        {
            _context = context;
            _paymentService = paymentService;
            _httpContextAccessor = httpContextAccessor;
            _momoSettings = momoSettings.Value;
        }

        public async Task<IActionResult> Index()
        {
            var cart = GetCart();
            var stories = await _context.Stories
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .Take(12)
                .Select(s => new StoryCatalogItem
                {
                    Id = s.Id,
                    StoryTitle = s.StoryName,
                    Price = s.Price
                })
                .ToListAsync();

            var model = new ShoppingCartViewModel
            {
                Cart = cart,
                Stories = stories,
                VipPackages = BuildVipPackages()
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Cart()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStory(Guid idStory, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;

            var story = await _context.Stories.AsNoTracking().FirstOrDefaultAsync(s => s.Id == idStory);
            if (story == null) return NotFound();

            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.StoryId == idStory);
            if (item == null)
            {
                cart.Items.Add(new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    StoryId = idStory,
                    StoryTitle = story.StoryName,
                    Price = story.Price,
                    Quantity = quantity
                });
            }
            else
            {
                item.Quantity += quantity;
            }

            SaveCart(cart);
            TempData["CartMessage"] = "Đã thêm vào giỏ hàng.";
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddVipPackage(string code)
        {
            var package = BuildVipPackages().FirstOrDefault(p => p.Code == code);
            if (package == null) return NotFound();

            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.StoryId == Guid.Empty && i.StoryTitle == package.Name);
            if (item == null)
            {
                cart.Items.Add(new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    StoryId = Guid.Empty,
                    StoryTitle = package.Name,
                    Price = package.Price,
                    Quantity = 1
                });
            }
            else
            {
                item.Quantity += 1;
            }

            SaveCart(cart);
            TempData["CartMessage"] = "Đã thêm gói VIP vào giỏ hàng.";
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(Guid itemId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return RedirectToAction(nameof(Index));

            if (quantity <= 0) cart.Items.Remove(item);
            else item.Quantity = quantity;

            SaveCart(cart);
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItem(Guid itemId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                cart.Items.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction(nameof(Cart));
        }

        [HttpGet]
        [Authorize]
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (cart.Items.Count == 0) return RedirectToAction(nameof(Index));
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayWithMomo()
        {
            var cart = GetCart();
            if (cart.Items.Count == 0) return RedirectToAction(nameof(Index));

            if (string.IsNullOrWhiteSpace(_momoSettings.AccessKey) ||
                string.IsNullOrWhiteSpace(_momoSettings.SecretKey) ||
                string.IsNullOrWhiteSpace(_momoSettings.PartnerCode))
            {
                TempData["PaymentError"] = "Cấu hình MoMo không hợp lệ.";
                return RedirectToAction(nameof(Checkout));
            }

            // BƯỚC 1: TẠO ĐƠN HÀNG
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty,
                PaymentMethod = "MoMo",
                PaymentStatus = "Pending",
                Note = BuildVipNote(cart),
                CreatedAt = DateTime.UtcNow,
                OrderDetails = new List<OrderDetail>() // Khởi tạo list chi tiết
            };

            var amount = cart.Items.Sum(i => i.Total);
            order.Amount = amount;
            order.Tax = amount * 0.1m;
            order.TotalAmount = order.Amount + order.Tax;

            // BƯỚC 2: THÊM CHI TIẾT (XỬ LÝ LỖI STORYID KHÔNG TỒN TẠI)
            foreach (var item in cart.Items)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    // QUAN TRỌNG: Nếu là VIP (Guid.Empty), phải để StoryId là NULL
                    // Bạn phải đảm bảo StoryId trong class OrderDetail là kiểu Guid? (nullable)
                    StoryId = item.StoryId == Guid.Empty ? null : item.StoryId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    Amount = item.Total,
                    Discount = 0
                });
            }

            // Lưu Order chính, nó sẽ tự kéo theo OrderDetails
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // BƯỚC 3: GỬI REQUEST THANH TOÁN
            var request = new Momopayment.MomoPaymentRequest
            {
                OrderId = order.Id.ToString(),
                FullName = order.UserId,
                OrderInfo = $"Thanh toán đơn hàng WibuHub #{order.Id}",
                Amount = (long)order.TotalAmount
            };

            var response = await _paymentService.CreateMomoPaymentAsync(request);

            if (response.ErrorCode == 0 && !string.IsNullOrWhiteSpace(response.PayUrl))
            {
                return Redirect(response.PayUrl);
            }

            TempData["PaymentError"] = response.Message;
            return RedirectToAction(nameof(Checkout));
        }

        [HttpGet]
        public IActionResult PaymentResult(int resultCode, string orderId)
        {
            if (resultCode == 0)
            {
                ViewBag.Message = "Thanh toán thành công!";
                HttpContext.Session.Remove(CartSessionKey);
            }
            else
            {
                ViewBag.Message = "Giao dịch không thành công.";
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> MomoCallback([FromBody] Momopayment.MomoCallbackRequest callback)
        {
            if (callback == null) return BadRequest();

            try
            {
                if (callback.ResultCode == 0)
                {
                    if (Guid.TryParse(callback.OrderId, out Guid orderGuid))
                    {
                        // Cần dùng Include nếu sau này muốn xử lý logic liên quan đến Details
                        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderGuid);

                        if (order != null && order.PaymentStatus != "Completed")
                        {
                            order.PaymentStatus = "Completed";
                            order.TransactionId = callback.TransId.ToString();
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                return NoContent();
            }
            catch
            {
                return StatusCode(500);
            }
        }

        #region Helpers
        private Cart GetCart()
        {
            var cart = HttpContext.Session.GetObject<Cart>(CartSessionKey);
            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<CartItem>()
                };
                SaveCart(cart);
            }
            return cart;
        }

        private void SaveCart(Cart cart)
        {
            cart.UpdatedAt = DateTime.UtcNow;
            HttpContext.Session.SetObject(CartSessionKey, cart);
        }

        private static List<VipPackageItem> BuildVipPackages()
        {
            return new List<VipPackageItem>
            {
                new VipPackageItem { Code = "VIP_MONTH",

                                     Name = "VIP 1 Tháng",

                                     Description = "Đọc không giới hạn trong 30 ngày",

                                     Price = 99000,

                                     DurationDays = 30 },
                new VipPackageItem { Code = "VIP_QUARTER",

                                     Name = "VIP 3 Tháng",

                                     Description = "Tiết kiệm hơn khi đăng ký 90 ngày",

                                     Price = 249000,

                                     DurationDays = 90 },
                new VipPackageItem { Code = "VIP_YEAR",

                                     Name = "VIP 12 Tháng",

                                     Description = "Gói tiết kiệm nhất cho fan truyện",

                                     Price = 799000,

                                     DurationDays = 365 }
            };
        }

        private static string BuildVipNote(Cart cart)
        {
            var vipItems = cart.Items.Where(i => i.StoryId == Guid.Empty).ToList();
            return vipItems.Count == 0 ? "Thanh toán truyện" : "Đăng ký VIP: " + string.Join(", ", vipItems.Select(i => i.StoryTitle));
        }
        #endregion
    }
}