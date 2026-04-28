using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Claims;
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

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 12;
            if (page < 1)
            {
                page = 1;
            }

            var cart = GetCart();
            var storiesQuery = _context.Stories
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt);

            var totalStories = await storiesQuery.CountAsync();
            var totalPages = totalStories == 0 ? 1 : (int)Math.Ceiling(totalStories / (double)pageSize);
            if (page > totalPages)
            {
                page = totalPages;
            }

            var stories = await storiesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StoryCatalogItem
                {
                    Id = s.Id,
                    StoryTitle = s.StoryName,
                    Price = s.Price
                })
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalStories = totalStories;

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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Orders(int page = 1)
        {
            var userName = User.Identity?.Name;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            const int pageSize = 10;

            await ExpireStalePendingOrdersAsync(userName, userId);

            if (page < 1)
            {
                page = 1;
            }

            var query = _context.Orders
                .AsNoTracking()
                .Where(o =>
                    (!string.IsNullOrWhiteSpace(userName) && o.UserId == userName)
                    || (!string.IsNullOrWhiteSpace(userId) && o.UserId == userId));

            var totalOrders = await query.CountAsync();
            var totalPages = totalOrders == 0 ? 1 : (int)Math.Ceiling(totalOrders / (double)pageSize);

            if (page > totalPages)
            {
                page = totalPages;
            }

            var orders = await query
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Story)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalOrders = totalOrders;

            return View(orders);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> PurchasedStories(int page = 1)
        {
            var userName = User.Identity?.Name;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            const int pageSize = 12;

            await ExpireStalePendingOrdersAsync(userName, userId);

            if (page < 1)
            {
                page = 1;
            }

            var purchasedStoriesQuery = _context.OrderDetails
                .AsNoTracking()
                .Where(d => d.StoryId.HasValue
                    && d.StoryId.Value != Guid.Empty
                    && d.Story != null
                    && d.Order.PaymentStatus == "Completed"
                    && ((!string.IsNullOrWhiteSpace(userName) && d.Order.UserId == userName)
                        || (!string.IsNullOrWhiteSpace(userId) && d.Order.UserId == userId)))
                .GroupBy(d => new
                {
                    StoryId = d.StoryId!.Value,
                    StoryTitle = d.Story != null ? d.Story.StoryName : (d.ItemName ?? "Story"),
                    CoverImage = d.Story != null ? d.Story.CoverImage : null
                })
                .Select(g => new PurchasedStoryItemViewModel
                {
                    StoryId = g.Key.StoryId,
                    StoryTitle = g.Key.StoryTitle,
                    CoverImage = g.Key.CoverImage,
                    TotalPurchasedQuantity = g.Sum(x => x.Quantity),
                    TotalSpent = g.Sum(x => x.Amount),
                    LastPurchasedAt = g.Max(x => x.Order.CreatedAt)
                });

            var totalItems = await purchasedStoriesQuery.CountAsync();
            var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)pageSize);

            if (page > totalPages)
            {
                page = totalPages;
            }

            var purchasedStories = await purchasedStoriesQuery
                .OrderByDescending(x => x.LastPurchasedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View(purchasedStories);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadStory(Guid storyId)
        {
            if (storyId == Guid.Empty)
            {
                return RedirectWithDownloadError("Liên kết tải truyện không hợp lệ.");
            }

            var userName = User.Identity?.Name;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var hasPurchased = await _context.Orders
                .AsNoTracking()
                .Where(o => o.PaymentStatus == "Completed"
                    && ((!string.IsNullOrWhiteSpace(userName) && o.UserId == userName)
                        || (!string.IsNullOrWhiteSpace(userId) && o.UserId == userId)))
                .SelectMany(o => o.OrderDetails)
                .AnyAsync(d => d.StoryId == storyId);

            if (!hasPurchased)
            {
                return RedirectWithDownloadError("Bạn chưa mua truyện này nên không thể tải xuống.");
            }

            var story = await _context.Stories
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == storyId && !s.IsDeleted);

            if (story == null)
            {
                return RedirectWithDownloadError("Truyện hiện không khả dụng để tải xuống.");
            }

            var chapters = await _context.Chapters
                .AsNoTracking()
                .Where(c => c.StoryId == storyId && !c.IsDeleted)
                .Include(c => c.Images)
                .OrderBy(c => c.ChapterNumber)
                .ToListAsync();

            if (chapters.Count == 0)
            {
                return RedirectWithDownloadError("Truyện chưa có chapter để tải xuống.");
            }

            using var archiveStream = new MemoryStream();
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
            {
                var storyFolder = SanitizeFileName(story.StoryName);
                if (string.IsNullOrWhiteSpace(storyFolder))
                {
                    storyFolder = "story";
                }

                var indexEntry = archive.CreateEntry($"{storyFolder}/README.txt");
                await using (var indexStream = indexEntry.Open())
                await using (var writer = new StreamWriter(indexStream, Encoding.UTF8))
                {
                    await writer.WriteLineAsync($"Story: {story.StoryName}");
                    await writer.WriteLineAsync($"Downloaded at (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                    await writer.WriteLineAsync($"Total chapters: {chapters.Count}");
                }

                using var httpClient = new HttpClient();

                foreach (var chapter in chapters)
                {
                    var chapterFileName = $"{chapter.ChapterNumber:0.##}-{SanitizeFileName(chapter.Name)}";
                    if (string.IsNullOrWhiteSpace(chapterFileName))
                    {
                        chapterFileName = chapter.Id.ToString();
                    }

                    var chapterFolder = $"{storyFolder}/{chapterFileName}";

                    if (!string.IsNullOrWhiteSpace(chapter.Content))
                    {
                        var contentEntry = archive.CreateEntry($"{chapterFolder}/content.txt");
                        await using var contentStream = contentEntry.Open();
                        await using var contentWriter = new StreamWriter(contentStream, Encoding.UTF8);
                        await contentWriter.WriteAsync(chapter.Content);
                    }

                    var images = chapter.Images?
                        .OrderBy(i => i.OrderIndex)
                        .ToList() ?? new List<ChapterImage>();

                    for (var imageIndex = 0; imageIndex < images.Count; imageIndex++)
                    {
                        var image = images[imageIndex];
                        var imageUrl = ResolveImageUrl(image.ImageUrl);
                        if (string.IsNullOrWhiteSpace(imageUrl))
                        {
                            continue;
                        }

                        try
                        {
                            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                            var extension = Path.GetExtension(image.ImageUrl);
                            if (string.IsNullOrWhiteSpace(extension))
                            {
                                extension = ".jpg";
                            }

                            var imageEntry = archive.CreateEntry($"{chapterFolder}/images/{imageIndex + 1:D3}{extension}");
                            await using var imageStream = imageEntry.Open();
                            await imageStream.WriteAsync(imageBytes, 0, imageBytes.Length);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            archiveStream.Position = 0;
            var outputName = $"{SanitizeFileName(story.StoryName)}-{DateTime.UtcNow:yyyyMMddHHmmss}.zip";
            return File(archiveStream.ToArray(), "application/zip", outputName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStory(Guid idStory, int quantity = 1)
        {
            quantity = 1;

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

                TempData["CartMessage"] = "Đã thêm vào giỏ hàng.";
            }
            else
            {
                item.Quantity = 1;
                TempData["CartMessage"] = "Truyện đã có trong giỏ hàng.";
            }

            SaveCart(cart);
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
        public async Task<IActionResult> UpdateQuantity(Guid itemId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return RedirectToAction(nameof(Index));

            if (item.StoryId != Guid.Empty)
            {
                if (quantity <= 0)
                {
                    cart.Items.Remove(item);
                    await MarkPendingOrderAsFailedIfNeededAsync();
                    SaveCart(cart);
                }

                return RedirectToAction(nameof(Cart));
            }

            if (quantity <= 0)
            {
                cart.Items.Remove(item);
                await MarkPendingOrderAsFailedIfNeededAsync();
            }
            else item.Quantity = quantity;

            SaveCart(cart);
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(Guid itemId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                cart.Items.Remove(item);
                await MarkPendingOrderAsFailedIfNeededAsync();
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
                TempData["TransferOrderId"] = order.Id.ToString();
                TempData["TransferAmount"] = order.TotalAmount.ToString("0");
                TempData["TransferPayUrl"] = response.PayUrl;
                return RedirectToAction(nameof(PaymentTransfer));
            }

            order.PaymentStatus = "Failed";
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove(PendingMomoOrderSessionKey);
            TempData["PaymentError"] = response.Message;
            return RedirectToAction(nameof(Checkout));
        }

        [HttpGet]
        [Authorize]
        public IActionResult PaymentTransfer()
        {
            if (TempData["TransferOrderId"] == null)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.OrderId = TempData["TransferOrderId"];
            ViewBag.Amount = TempData["TransferAmount"];
            ViewBag.PayUrl = TempData["TransferPayUrl"];

            // Keep TempData in case of refresh
            TempData.Keep("TransferOrderId");
            TempData.Keep("TransferAmount");
            TempData.Keep("TransferPayUrl");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PaymentResult([FromQuery] int resultCode, [FromQuery] string orderId)
        {
            ViewBag.IsSuccess = false;
            ViewBag.HasVip = false;
            ViewBag.HasStory = false;
            ViewBag.TransactionId = orderId;

            if (resultCode == 0)
            {
                if (!string.IsNullOrEmpty(orderId) && Guid.TryParse(orderId, out Guid orderGuid))
                {
                    var order = await _context.Orders
                        .AsNoTracking()
                        .Include(o => o.OrderDetails)
                        .FirstOrDefaultAsync(o => o.Id == orderGuid);

                    if (order != null)
                    {
                        ViewBag.HasVip = order.OrderDetails.Any(d => d.StoryId == null || d.StoryId == Guid.Empty);
                        ViewBag.HasStory = order.OrderDetails.Any(d => d.StoryId != null && d.StoryId != Guid.Empty);
                        ViewBag.TransactionId = string.IsNullOrWhiteSpace(order.TransactionId)
                            ? order.Id.ToString()
                            : order.TransactionId;

                        if (string.Equals(order.PaymentStatus, "Completed", StringComparison.OrdinalIgnoreCase))
                        {
                            ViewBag.IsSuccess = true;
                            ViewBag.Message = "Thanh toán thành công. Hệ thống đã nâng cấp và ghi nhận đơn hàng của bạn.";

                            if (User?.Identity?.IsAuthenticated == true)
                            {
                                var userManager = HttpContext.RequestServices.GetService<UserManager<StoryUser>>();
                                var currentUser = await ResolveOrderUserAsync(userManager, order.UserId);
                                if (currentUser != null)
                                {
                                    await CreateOrderNotificationIfNeededAsync(currentUser, order);
                                    await _context.SaveChangesAsync();
                                }
                            }

                            HttpContext.Session.Remove(CartSessionKey);
                            HttpContext.Session.Remove(PendingMomoOrderSessionKey);
                        }
                        else
                        {
                            ViewBag.Message = "Đã nhận kết quả thanh toán, hệ thống đang chờ xác nhận từ cổng MoMo. Vui lòng thử tải lại sau vài giây.";
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Không tìm thấy đơn hàng tương ứng.";
                    }
                }
                else
                {
                    ViewBag.Message = "Thông tin đơn hàng không hợp lệ.";
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
                if (!IsValidMomoCallbackSignature(callback))
                {
                    return NoContent();
                }

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
                            var totalVipDaysBought = CalculateVipDaysFromOrderDetails(order.OrderDetails);

                            // 3. Nếu có mua VIP, cộng ngày cho User
                            if (totalVipDaysBought > 0 && !string.IsNullOrEmpty(order.UserId))
                            {
                                var userManager = HttpContext.RequestServices.GetService<UserManager<StoryUser>>();
                                var signInManager = HttpContext.RequestServices.GetService<SignInManager<StoryUser>>();

                                if (userManager != null)
                                {
                                    var user = await ResolveOrderUserAsync(userManager, order.UserId);
                                    if (user != null)
                                    {
                                        var currentVipEnd = user.VipExpireDate.HasValue && user.VipExpireDate.Value > DateTime.UtcNow
                                                            ? user.VipExpireDate.Value
                                                            : DateTime.UtcNow;

                                        user.VipExpireDate = currentVipEnd.AddDays(totalVipDaysBought);
                                        await userManager.UpdateAsync(user);

                                        if (signInManager != null)
                                        {
                                            await signInManager.RefreshSignInAsync(user);
                                        }

                                        await GrantOrderRewardsAsync(user.Id, order.TotalAmount);
                                        await CreateOrderNotificationIfNeededAsync(user, order);
                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(order.UserId))
                            {
                                var userManager = HttpContext.RequestServices.GetService<UserManager<StoryUser>>();
                                if (userManager != null)
                                {
                                    var user = await ResolveOrderUserAsync(userManager, order.UserId);
                                    if (user != null)
                                    {
                                        await GrantOrderRewardsAsync(user.Id, order.TotalAmount);
                                        await CreateOrderNotificationIfNeededAsync(user, order);
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

        private async Task MarkPendingOrderAsFailedIfNeededAsync()
        {
            if (!Guid.TryParse(HttpContext.Session.GetString(PendingMomoOrderSessionKey), out var pendingOrderId))
            {
                return;
            }

            var pendingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.Id == pendingOrderId);
            if (pendingOrder != null && pendingOrder.PaymentStatus == "Pending")
            {
                pendingOrder.PaymentStatus = "Failed";
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.Remove(PendingMomoOrderSessionKey);
        }

        private async Task ExpireStalePendingOrdersAsync(string? userName, string? userId)
        {
            var expireBefore = DateTime.UtcNow.AddMinutes(-30);

            await _context.Orders
                .Where(o => o.PaymentStatus == "Pending"
                    && o.CreatedAt <= expireBefore
                    && ((!string.IsNullOrWhiteSpace(userName) && o.UserId == userName)
                        || (!string.IsNullOrWhiteSpace(userId) && o.UserId == userId)))
                .ExecuteUpdateAsync(setters => setters.SetProperty(o => o.PaymentStatus, "Failed"));
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

        private static int CalculateVipDaysFromOrderDetails(IEnumerable<OrderDetail> orderDetails)
        {
            var availableVipPackages = BuildVipPackages();
            var totalVipDaysBought = 0;

            foreach (var detail in orderDetails)
            {
                if (detail.StoryId != null && detail.StoryId != Guid.Empty)
                {
                    continue;
                }

                var pkg = availableVipPackages.FirstOrDefault(p =>
                    !string.IsNullOrWhiteSpace(detail.ItemName)
                    && string.Equals(p.Name, detail.ItemName, StringComparison.OrdinalIgnoreCase));

                if (pkg == null)
                {
                    pkg = availableVipPackages.FirstOrDefault(p => p.Price == detail.UnitPrice);
                }

                if (pkg != null)
                {
                    totalVipDaysBought += (pkg.DurationDays * Math.Max(1, detail.Quantity));
                }
            }

            return totalVipDaysBought;
        }

        private string ResolveImageUrl(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return string.Empty;
            }

            if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.ToString();
            }

            return $"{Request.Scheme}://{Request.Host}{(imageUrl.StartsWith('/') ? imageUrl : '/' + imageUrl)}";
        }

        private static string SanitizeFileName(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidRegex = $"[{invalidChars}]";
            return Regex.Replace(input.Trim(), invalidRegex, "_");
        }

        private IActionResult RedirectWithDownloadError(string message)
        {
            TempData["DownloadError"] = message;

            var referer = Request.Headers.Referer.ToString();
            if (!string.IsNullOrWhiteSpace(referer)
                && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
            {
                var path = refererUri.AbsolutePath;
                if (path.Contains("/ShoppingCart/Orders", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction(nameof(Orders));
                }

                if (path.Contains("/ShoppingCart/PurchasedStories", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction(nameof(PurchasedStories));
                }
            }

            return RedirectToAction(nameof(PurchasedStories));
        }

        private async Task GrantOrderRewardsAsync(string userId, decimal totalAmount)
        {
            if (string.IsNullOrWhiteSpace(userId) || totalAmount <= 0)
            {
                return;
            }

            var expAdded = Math.Max(1, (int)Math.Floor(totalAmount / 10000m));
            var pointsAdded = Math.Max(3, (int)Math.Floor(totalAmount / 20000m));

            await _rewardService.AddExpAndPointsAsync(userId, expAdded, pointsAdded);
        }

        private async Task CreateOrderNotificationIfNeededAsync(StoryUser user, Order order)
        {
            if (!Guid.TryParse(user.Id, out var userId))
            {
                return;
            }

            var orderCode = order.Id.ToString()[..8].ToUpperInvariant();
            var targetUrl = $"/ShoppingCart/PaymentResult?resultCode=0&orderId={order.Id}";
            var title = $"Đơn hàng #{orderCode} đã thanh toán";

            var existed = await _context.Notifications.AnyAsync(n =>
                n.UserId == userId
                && n.Title == title
                && n.TargetUrl == targetUrl);

            if (existed)
            {
                return;
            }

            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = $"Thanh toán thành công đơn hàng {orderCode}. Tổng tiền: {order.TotalAmount:N0}đ",
                TargetUrl = targetUrl,
                IsRead = false,
                CreateDate = DateTime.UtcNow
            });
        }

        private async Task<bool> ShouldGrantRewardsForOrderAsync(Order order)
        {
            if (string.IsNullOrWhiteSpace(order.UserId))
            {
                return false;
            }

            var userManager = HttpContext.RequestServices.GetService<UserManager<StoryUser>>();
            var user = await ResolveOrderUserAsync(userManager, order.UserId);
            if (user == null || !Guid.TryParse(user.Id, out var userGuid))
            {
                return false;
            }

            var orderCode = order.Id.ToString()[..8].ToUpperInvariant();
            var targetUrl = $"/ShoppingCart/PaymentResult?resultCode=0&orderId={order.Id}";
            var title = $"Đơn hàng #{orderCode} đã thanh toán";

            var existed = await _context.Notifications.AnyAsync(n =>
                n.UserId == userGuid
                && n.Title == title
                && n.TargetUrl == targetUrl);

            return !existed;
        }

        private async Task<StoryUser?> ResolveOrderUserAsync(UserManager<StoryUser>? userManager, string? orderUserId)
        {
            if (userManager == null || string.IsNullOrWhiteSpace(orderUserId))
            {
                return null;
            }

            if (Guid.TryParse(orderUserId, out _))
            {
                var byId = await userManager.FindByIdAsync(orderUserId);
                if (byId != null)
                {
                    return byId;
                }
            }

            return await userManager.FindByNameAsync(orderUserId);
        }

        private bool IsValidMomoCallbackSignature(Momopayment.MomoCallbackRequest callback)
        {
            if (string.IsNullOrWhiteSpace(callback.Signature)
                || string.IsNullOrWhiteSpace(callback.PartnerCode)
                || !string.Equals(callback.PartnerCode, _momoSettings.PartnerCode, StringComparison.Ordinal))
            {
                return false;
            }

            var rawSignature = $"accessKey={_momoSettings.AccessKey}" +
                               $"&amount={callback.Amount}" +
                               $"&extraData={callback.ExtraData ?? string.Empty}" +
                               $"&message={callback.Message}" +
                               $"&orderId={callback.OrderId}" +
                               $"&orderInfo={callback.OrderInfo}" +
                               $"&orderType={callback.OrderType}" +
                               $"&partnerCode={callback.PartnerCode}" +
                               $"&payType={callback.PayType}" +
                               $"&requestId={callback.RequestId}" +
                               $"&responseTime={callback.ResponseTime}" +
                               $"&resultCode={callback.ResultCode}" +
                               $"&transId={callback.TransId}";

            var expectedSignature = ComputeHmacSha256(rawSignature, _momoSettings.SecretKey);
            return string.Equals(callback.Signature, expectedSignature, StringComparison.OrdinalIgnoreCase);
        }

        private static string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
        }
        #endregion
    }
}