using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using WibuHub.MVC.ExtensionsMethod;
using WibuHub.MVC.ViewModels.ShoppingCart;

namespace WibuHub.MVC.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string CartSessionKey = "CartSession";

        public PaymentController(IHttpClientFactory httpClientFactory, ILogger<PaymentController> logger, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _configuration = configuration;
        }

        // GET: Payment
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<Cart>(CartSessionKey);
            if (cart == null || !cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty";
                return RedirectToAction("Index", "ShoppingCart");
            }
            return View(cart);
        }

        // POST: Payment/CreateMomoPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMomoPayment(decimal amount, string orderInfo)
        {
            // Validate amount
            if (amount <= 0 || amount > 1000000000)
            {
                TempData["ErrorMessage"] = "Invalid payment amount";
                return RedirectToAction("Error");
            }

            // Validate orderInfo
            if (string.IsNullOrWhiteSpace(orderInfo) || orderInfo.Length > 200)
            {
                TempData["ErrorMessage"] = "Invalid order information";
                return RedirectToAction("Error");
            }

            try
            {
                // Use configured API URL instead of building from request
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                var apiUrl = $"{apiBaseUrl}/api/payments/momo";
                
                var requestData = new
                {
                    amount = amount,
                    orderInfo = orderInfo
                };

                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    if (result.TryGetProperty("data", out var data) && 
                        data.TryGetProperty("payUrl", out var payUrl))
                    {
                        var payUrlString = payUrl.GetString();
                        if (!string.IsNullOrEmpty(payUrlString))
                        {
                            return Redirect(payUrlString);
                        }
                    }
                }

                // Extract error message from response if available
                try
                {
                    var errorResult = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    if (errorResult.TryGetProperty("message", out var errorMsg))
                    {
                        TempData["ErrorMessage"] = errorMsg.GetString();
                    }
                }
                catch
                {
                    // Use generic error if parsing fails
                }

                if (TempData["ErrorMessage"] == null)
                {
                    TempData["ErrorMessage"] = "Unable to create payment request";
                }
                return RedirectToAction("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo payment");
                TempData["ErrorMessage"] = "Unable to process payment. Please try again.";
                return RedirectToAction("Error");
            }
        }

        // GET: Payment/Success
        public IActionResult Success()
        {
            return View();
        }

        // GET: Payment/Error
        public IActionResult Error()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"]?.ToString() ?? "Payment failed. Please try again.";
            return View();
        }
    }
}
