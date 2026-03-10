using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Configuration;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.MVC.Customer.ExtensionsMethod;
using WibuHub.MVC.Customer.Models.ShoppingCart;
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
            var chapters = await _context.Chapters
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .Take(12)
                .Select(c => new ChapterCatalogItem
                {
                    Id = c.Id,
                    ChapterName = c.Name,
                    StoryTitle = c.StoryName,
                    Price = c.Price
                })
                .ToListAsync();

            var model = new ShoppingCartViewModel
            {
                Cart = cart,
                Chapters = chapters,
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
        public async Task<IActionResult> AddChapter(Guid idChapter, int quantity = 1)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            var chapter = await _context.Chapters.AsNoTracking().FirstOrDefaultAsync(c => c.Id == idChapter);
            if (chapter == null)
            {
                return NotFound();
            }

            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ChapterId == idChapter);
            if (item == null)
            {
                cart.Items.Add(new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ChapterId = idChapter,
                    ChapterName = chapter.Name,
                    StoryTitle = chapter.StoryName,
                    Price = chapter.Price,
                    Quantity = quantity
                });
            }
            else
            {
                item.Quantity += quantity;
            }

            SaveCart(cart);
            TempData["CartMessage"] = "Ð? thêm vào gi? hàng.";
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddVipPackage(string code)
        {
            var package = BuildVipPackages().FirstOrDefault(p => p.Code == code);
            if (package == null)
            {
                return NotFound();
            }

            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ChapterId == Guid.Empty && i.ChapterName == package.Name);
            if (item == null)
            {
                cart.Items.Add(new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ChapterId = Guid.Empty,
                    ChapterName = package.Name,
                    StoryTitle = "Gói VIP",
                    Price = package.Price,
                    Quantity = 1
                });
            }
            else
            {
                item.Quantity += 1;
            }

            SaveCart(cart);
            TempData["CartMessage"] = "Ð? thêm gói VIP vào gi? hàng.";
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(Guid itemId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }

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
        public IActionResult Checkout()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = Url.Action(nameof(Checkout), "ShoppingCart") });
            }

            var cart = GetCart();
            if (cart.Items.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayWithMomo()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = Url.Action(nameof(Checkout), "ShoppingCart") });
            }

            var cart = GetCart();
            if (cart.Items.Count == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(_momoSettings.AccessKey)
                || string.IsNullOrWhiteSpace(_momoSettings.SecretKey)
                || string.IsNullOrWhiteSpace(_momoSettings.PartnerCode))
            {
                TempData["PaymentError"] = "Chýa c?u h?nh MoMo. Vui l?ng c?p nh?t MomoSettings trong appsettings.json.";
                return RedirectToAction(nameof(Checkout));
            }

            var order = new Order
            {
                UserId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty,
                Phone = string.Empty,
                Email = string.Empty,
                ShippingAddress = string.Empty,
                PaymentMethod = "MoMo",
                PaymentStatus = "Pending",
                Note = BuildVipNote(cart)
            };

            var amount = cart.Items.Sum(i => i.Total);
            order.Amount = amount;
            order.Tax = amount * 0.1m;
            order.TotalAmount = order.Amount + order.Tax;

            await _context.Orders.AddAsync(order);

            foreach (var item in cart.Items.Where(i => i.ChapterId != Guid.Empty))
            {
                var detail = new OrderDetail
                {
                    OrderId = order.Id,
                    ChapterId = item.ChapterId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    Amount = item.Total,
                    Discount = 0
                };
                await _context.OrderDetails.AddAsync(detail);
            }

            await _context.SaveChangesAsync();

            var request = new Momopayment.MomoPaymentRequest
            {
                OrderId = order.Id.ToString(),
                FullName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Customer",
                OrderInfo = $"Thanh toán ðõn hàng {order.Id}",
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
        public IActionResult PaymentResult()
        {
            return View();
        }

        [HttpPost]
        public IActionResult MomoCallback([FromBody] Momopayment.MomoCallbackRequest callback)
        {
            return Ok();
        }

        private Cart GetCart()
        {
            var cart = HttpContext.Session.GetObject<Cart>(CartSessionKey);
            if (cart != null)
            {
                return cart;
            }

            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<CartItem>()
            };

            SaveCart(cart);
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
                new VipPackageItem
                {
                    Code = "VIP_MONTH",
                    Name = "VIP 1 Tháng",
                    Description = "Ð?c không gi?i h?n trong 30 ngày",
                    Price = 99000,
                    DurationDays = 30
                },
                new VipPackageItem
                {
                    Code = "VIP_QUARTER",
                    Name = "VIP 3 Tháng",
                    Description = "Ti?t ki?m hõn khi ðãng k? 90 ngày",
                    Price = 249000,
                    DurationDays = 90
                },
                new VipPackageItem
                {
                    Code = "VIP_YEAR",
                    Name = "VIP 12 Tháng",
                    Description = "Gói ti?t ki?m nh?t cho fan truy?n",
                    Price = 799000,
                    DurationDays = 365
                }
            };
        }

        private static string BuildVipNote(Cart cart)
        {
            var vipItems = cart.Items.Where(i => i.ChapterId == Guid.Empty).ToList();
            if (vipItems.Count == 0)
            {
                return string.Empty;
            }

            return "Ðãng k? VIP: " + string.Join(", ", vipItems.Select(i => $"{i.ChapterName} x{i.Quantity}"));
        }
    }
}
