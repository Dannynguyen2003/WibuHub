using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.MVC.Customer.Models;

namespace WibuHub.MVC.Customer.Controllers
{
    public class HomeController : Controller
    {
        private const string StoriesEndpoint = "api/stories";
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("WibuHubApi");
                if (httpClient.BaseAddress is null)
                {
                    _logger.LogWarning("WibuHubApi BaseAddress is not configured.");
                    return View(new List<StoryDto>());
                }

                var stories = await httpClient.GetFromJsonAsync<List<StoryDto>>(StoriesEndpoint);
                return View(stories ?? new List<StoryDto>());
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch stories from API");
                return View(new List<StoryDto>());
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request to fetch stories from API timed out");
                return View(new List<StoryDto>());
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse stories API response");
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
