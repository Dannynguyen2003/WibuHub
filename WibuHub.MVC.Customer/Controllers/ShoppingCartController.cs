using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WibuHub.ApplicationCore.Configuration;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Entities;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.DataLayer;
using WibuHub.MVC.Customer.ExtensionsMethod;
using WibuHub.MVC.Customer.ViewModels.ShoppingCart;
using WibuHub.MVC.ViewModels.ShoppingCart;
using WibuHub.Service.Implementation;
using WibuHub.Service.Interface;

namespace WibuHub.MVC.Customer.Controllers
{
    public class ShoppingCartController : Controller
    {
        private const string CartSessionKey = "CartSession";
        private const string PendingMomoOrderSessionKey = "PendingMomoOrderId";
        private readonly StoryDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly IRewardService _rewardService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MomoSettings _momoSettings;

        public ShoppingCartController(
            StoryDbContext context,
            IPaymentService paymentService,
            IRewardService rewardService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<MomoSettings> momoSettings)
        {
            _context = context;
            _paymentService = paymentService;
            _rewardService = rewardService;
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

            if (Guid.TryParse(HttpContext.Session.GetString(PendingMomoOrderSessionKey), out var pendingOrderId))
            {
                var pendingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.Id == pendingOrderId);
                if (pendingOrder != null && pendingOrder.PaymentStatus == "Pending")
                {
                    pendingOrder.PaymentStatus = "Cancelled";
                    await _context.SaveChangesAsync();
                }
            }

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
                OrderDetails = new List<OrderDetail>()
            };

            var amount = cart.Items.Sum(i => i.Total);
            order.Amount = amount;
            order.Tax = amount * 0.1m;
            order.TotalAmount = order.Amount + order.Tax;

            // BƯỚC 2: THÊM CHI TIẾT VÀO ĐƠN HÀNG
            foreach (var item in cart.Items)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    StoryId = item.StoryId == Guid.Empty ? null : item.StoryId,
                    ItemName = item.StoryTitle,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    Amount = item.Total,
                    Discount = 0
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString(PendingMomoOrderSessionKey, order.Id.ToString());

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

            order.PaymentStatus = "Failed";
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove(PendingMomoOrderSessionKey);
            TempData["PaymentError"] = response.Message;
            return RedirectToAction(nameof(Checkout));
        }

        [HttpGet]
        public async Task<IActionResult> PaymentResult([FromQuery] int resultCode, [FromQuery] string orderId)
        {
            if (resultCode == 0)
            {
                ViewBag.Message = "Hệ thống đã xác nhận thanh toán và đang nâng cấp VIP cho bạn...";
                HttpContext.Session.Remove(CartSessionKey);
                HttpContext.Session.Remove(PendingMomoOrderSessionKey);

                if (!string.IsNullOrEmpty(orderId) && Guid.TryParse(orderId, out Guid orderGuid))
                {
                    var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == orderGuid);

                    if (order != null && order.PaymentStatus != "Completed")
                    {
                        order.PaymentStatus = "Completed";

                        var userManager = HttpContext.RequestServices.GetService<UserManager<StoryUser>>();
                        var signInManager = HttpContext.RequestServices.GetService<SignInManager<StoryUser>>();
                        var currentUser = await userManager.GetUserAsync(User);

                        if (currentUser != null)
                        {
                            var availableVipPackages = BuildVipPackages();
                            int totalVipDaysBought = 0;

                            foreach (var detail in order.OrderDetails)
                            {
                                if (detail.StoryId == null || detail.StoryId == Guid.Empty)
                                {
                                    var pkg = availableVipPackages.FirstOrDefault(p => p.Price == detail.UnitPrice);
                                    if (pkg != null)
                                    {
                                        totalVipDaysBought += (pkg.DurationDays * detail.Quantity);
                                    }
                                }
                            }

                            if (totalVipDaysBought > 0)
                            {
                                var currentVipEnd = currentUser.VipExpireDate.HasValue && currentUser.VipExpireDate.Value > DateTime.UtcNow
                                                    ? currentUser.VipExpireDate.Value
                                                    : DateTime.UtcNow;

                                currentUser.VipExpireDate = currentVipEnd.AddDays(totalVipDaysBought);

                                // Đã xóa dòng gán IsVip = true vì property này là ReadOnly tự tính toán
                                await userManager.UpdateAsync(currentUser);

                                // Làm mới Cookie trình duyệt
                                if (signInManager != null)
                                {
                                    await signInManager.RefreshSignInAsync(currentUser);
                                }
                            }

                            await GrantOrderRewardsAsync(currentUser.Id, order.TotalAmount);
                        }

                        await _context.SaveChangesAsync();
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(orderId) && Guid.TryParse(orderId, out Guid failedOrderId))
                {
                    var failedOrder = await _context.Orders.FirstOrDefaultAsync(o => o.Id == failedOrderId);
                    if (failedOrder != null && failedOrder.PaymentStatus == "Pending")
                    {
                        failedOrder.PaymentStatus = "Cancelled";
                        await _context.SaveChangesAsync();
                    }
                }

                HttpContext.Session.Remove(PendingMomoOrderSessionKey);
                ViewBag.Message = "Giao dịch không thành công hoặc đã bị hủy.";
            }

            return View();
        }

        // HÀM XỬ LÝ CALLBACK TỪ MOMO
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> MomoCallback([FromBody] Momopayment.MomoCallbackRequest callback)
        {
            if (callback == null) return BadRequest();

            try
            {
                if (callback.ResultCode == 0) // Giao dịch MoMo thành công
                {
                    if (Guid.TryParse(callback.OrderId, out Guid orderGuid))
                    {
                        var order = await _context.Orders
                                                  .Include(o => o.OrderDetails)
                                                  .FirstOrDefaultAsync(o => o.Id == orderGuid);

                        if (order != null && order.PaymentStatus != "Completed")
                        {
                            // 1. Cập nhật trạng thái đơn hàng
                            order.PaymentStatus = "Completed";
                            order.TransactionId = callback.TransId.ToString();

                            // 2. LOGIC CẤP QUYỀN VIP
                            var availableVipPackages = BuildVipPackages();
                            int totalVipDaysBought = 0;

                            foreach (var detail in order.OrderDetails)
                            {
                                if (detail.StoryId == null || detail.StoryId == Guid.Empty)
                                {
                                    var pkg = availableVipPackages.FirstOrDefault(p => p.Name == detail.ItemName);

                                    if (pkg == null)
                                    {
                                        pkg = availableVipPackages.FirstOrDefault(p => p.Price == detail.UnitPrice);
                                    }

                                    if (pkg != null)
                                    {
                                        totalVipDaysBought += (pkg.DurationDays * detail.Quantity);
                                    }
                                }
                                else
                                {
                                    // Xử lý lưu Truyện vào lịch sử mua nếu có
                                }
                            }

                            // 3. Nếu có mua VIP, cộng ngày cho User
                            if (totalVipDaysBought > 0 && !string.IsNullOrEmpty(order.UserId))
                            {
                                var userManager = HttpContext.RequestServices.GetService<UserManager<StoryUser>>();

                                if (userManager != null)
                                {
                                    var user = await userManager.FindByNameAsync(order.UserId);
                                    if (user != null)
                                    {
                                        var currentVipEnd = user.VipExpireDate.HasValue && user.VipExpireDate.Value > DateTime.UtcNow
                                                            ? user.VipExpireDate.Value
                                                            : DateTime.UtcNow;

                                        user.VipExpireDate = currentVipEnd.AddDays(totalVipDaysBought);
                                        await userManager.UpdateAsync(user);

                                        await GrantOrderRewardsAsync(user.Id, order.TotalAmount);
                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(order.UserId))
                            {
                                var userManager = HttpContext.RequestServices.GetService<UserManager<StoryUser>>();
                                if (userManager != null)
                                {
                                    var user = await userManager.FindByNameAsync(order.UserId);
                                    if (user != null)
                                    {
                                        await GrantOrderRewardsAsync(user.Id, order.TotalAmount);
                                    }
                                }
                            }

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

        private async Task GrantOrderRewardsAsync(string userId, decimal totalAmount)
        {
            if (string.IsNullOrWhiteSpace(userId) || totalAmount <= 0)
            {
                return;
            }

            var expAdded = Math.Max(1, (int)Math.Floor(totalAmount / 10000m));
            var pointsAdded = Math.Max(1, (int)Math.Floor(totalAmount / 50000m));

            await _rewardService.AddExpAndPointsAsync(userId, expAdded, pointsAdded);
        }
        #endregion
    }
}