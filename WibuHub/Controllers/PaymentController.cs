using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace WibuHub.MVC.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;

        public PaymentController(IHttpClientFactory httpClientFactory, ILogger<PaymentController> logger, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _configuration = configuration;
        }

        // GET: Payment
        public IActionResult Index()
        {
            return View();
        }

        // POST: Payment/CreateMomoPayment
        [HttpPost]
        public async Task<IActionResult> CreateMomoPayment(decimal amount, string orderInfo)
        {
            try
            {
                var apiUrl = $"{Request.Scheme}://{Request.Host}/api/payments/momo";
                
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
                        return Redirect(payUrl.GetString() ?? "/Payment/Error");
                    }
                }

                TempData["ErrorMessage"] = "Unable to create payment request";
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
