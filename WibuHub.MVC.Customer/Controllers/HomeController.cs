using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using WibuHub.MVC.Customer.DTOs;
using WibuHub.MVC.Customer.Models;

namespace WibuHub.MVC.Customer.Controllers
{
    public class HomeController : Controller
    {
        private const string CartSessionKey = "CartSession";
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost("api/cart/add")]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart([FromBody] AddToCartRequest request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Dữ liệu giỏ hàng không hợp lệ." });
            }

            var cart = HttpContext.Session.GetString(CartSessionKey);
            Dictionary<int, int> cartItems;
            try
            {
                cartItems = string.IsNullOrWhiteSpace(cart)
                    ? new Dictionary<int, int>()
                    : JsonSerializer.Deserialize<Dictionary<int, int>>(cart) ?? new Dictionary<int, int>();
            }
            catch (JsonException)
            {
                return BadRequest(new { message = "Giỏ hàng bị lỗi, vui lòng thử lại." });
            }

            if (cartItems.ContainsKey(request.ChapterId))
            {
                cartItems[request.ChapterId] += request.Quantity;
            }
            else
            {
                cartItems[request.ChapterId] = request.Quantity;
            }

            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cartItems));

            return Ok(new
            {
                message = "Đã thêm chương vào giỏ hàng.",
                totalItems = cartItems.Values.Sum()
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
