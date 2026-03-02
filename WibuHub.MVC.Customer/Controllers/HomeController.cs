using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Diagnostics;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.MVC.Customer.Models;

namespace WibuHub.MVC.Customer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var apiBaseUrl = _configuration["ApiBaseUrl"];
            if (string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                return View(new List<StoryDto>());
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var stories = await httpClient.GetFromJsonAsync<List<StoryDto>>($"{apiBaseUrl}/api/stories");
                return View(stories ?? new List<StoryDto>());
            }
            catch
            {
                return View(new List<StoryDto>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
